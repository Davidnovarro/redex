using Newtonsoft.Json;
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
        
        using var fileWriter = new MultiThreadFileWriter(options.FilePath);

        var exporter = new DefaultExporter(db, options);

        while (!exporter.IsFinished)
        {
            var list = await exporter.ExportAsync();
            
            while (fileWriter.Queue.Count > options.ScanCountPerPage * 3)
                Thread.Sleep(10);

            foreach (var ex in list)
            {
                fileWriter.WriteLine(JsonConvert.SerializeObject(ex));
            }
        }

        //Wait for all lines to be written
        while (!fileWriter.Queue.IsEmpty)
        {
            Thread.Sleep(1);
        }

        //Queue is empty, now we can call the Cancellation
        await fileWriter.CancellationTokenSource.CancelAsync();
        await fileWriter.task;

        Console.WriteLine("Finished!");
        return 0;
    }
}