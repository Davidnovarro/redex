﻿using Newtonsoft.Json;
using StackExchange.Redis;

namespace RedEx;

public static class Import
{
    public static async Task<int> Run(ImportOptions options)
    {
        options.Init();
        
        ConfigurationOptions redisConfiguration = new()
        {
            ConnectRetry = 4,
            ConnectTimeout = 5000,
            AsyncTimeout = 30000,
            SyncTimeout = 30000,
            EndPoints = new EndPointCollection() { options.Endpoint }
        };

        var multiplexer = await ConnectionMultiplexer.ConnectAsync(redisConfiguration);
        IDatabase db = multiplexer.GetDatabase(options.DB);

        Console.WriteLine($"Options: {JsonConvert.SerializeObject(options)}");

        var totalLineCount = (int)Tools.CountLines(options.FilePath);
        using var progressInfo = new ProgressInfo(totalLineCount, "Importing: {0} of {1}");
        
        using var fileReader = new MultiThreadFileReader(options.FilePath, options.BatchCount * 2);

        var tasks = new List<Task>();
        
        while (!fileReader.task.IsCompleted)
        {
            while (fileReader.Queue.Count < options.BatchCount)
            {
                Thread.Sleep(5);
                if(fileReader.task.IsCompleted)
                    break;
            }
        
            var batch = db.CreateBatch();
            
            while (!fileReader.Queue.IsEmpty && tasks.Count < options.BatchCount)
            {
                fileReader.Queue.TryDequeue(out var line);
                ExportValue ex = JsonConvert.DeserializeObject<ExportValue>(line);

                if (options.ReplaceKeys)
                    tasks.Add(batch.ExecuteAsync("RESTORE", ex.k, ex.ttl > 0 ? ex.ttl : 0, ex.v, "REPLACE"));
                else
                    tasks.Add(batch.ExecuteAsync("RESTORE", ex.k, ex.ttl > 0 ? ex.ttl : 0, ex.v));
            }
            
            batch.Execute();
            await Task.WhenAll(tasks);
            progressInfo.Tick(progressInfo.bar.CurrentTick + tasks.Count);
            tasks.Clear();
        }
        
        progressInfo.Tick(progressInfo.bar.MaxTicks);
        return 0;
    }
}