﻿using System.Diagnostics;
using Newtonsoft.Json;
using Party.Utility;
using ShellProgressBar;
using StackExchange.Redis;

namespace RedEx;

public static class Export
{
    public static async Task<int> Run(ExportOptions options)
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
        
        using var fileWriter = new MultiThreadExportValueWriter(options.FilePath);

        int totalKeyCount = 0;
        
        foreach (var server in multiplexer.GetServers())
        {
            if(server.IsReplica)
                continue;
            
            totalKeyCount += (int)await server.ExecuteAsync("DBSIZE");
        }
        
        using var progressInfo = new ProgressInfo(totalKeyCount, "Exporting: {0} of {1}");
        
        foreach (var server in multiplexer.GetServers())
        {
            if(server.IsReplica)
                continue;
            
            var exporter = new DefaultExporter(db, server, options);

            while (!exporter.IsFinished)
            {
                var list = await exporter.ExportAsync();

                while (fileWriter.Queue.Count > options.ScanCountPerPage * 4)
                    Thread.Sleep(10);

                foreach (var ex in list)
                {
                    if(ex.v == null) //Skip if the key was removed or expired, it might be because there was a delay between the SCAN and DUMP commands.  
                        continue;
                    
                    fileWriter.WriteLine(ex);
                }
                
                progressInfo.Tick(progressInfo.bar.CurrentTick + list.Count);
            }
        }
        
        progressInfo.Tick(progressInfo.bar.MaxTicks);

        //Wait for all lines to be written
        while (!fileWriter.Queue.IsEmpty)
        {
            Thread.Sleep(1);
        }

        //Queue is empty, now we can call the Cancellation
        await fileWriter.CancellationTokenSource.CancelAsync();
        await fileWriter.task;
        return 0;
    }
}