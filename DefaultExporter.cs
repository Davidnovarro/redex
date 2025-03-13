using Newtonsoft.Json;
using StackExchange.Redis;

namespace RedEx;

public class DefaultExporter(IDatabase redisDb, ExportOptions options)
{
    protected readonly IDatabase DB = redisDb;
    protected readonly ExportOptions Options = options;
    protected readonly List<ExportValue> export = [];
    protected readonly List<Task> tasks = [];

    public bool IsFinished => cursor == 0;
    
    protected ulong? cursor;
    
    public virtual async Task<List<ExportValue>> ExportAsync()
    {
        if (IsFinished)
        {
            throw new Exception($"Export is already finished");
        }
        
        export.Clear();
        tasks.Clear();
        cursor ??= 0;

        RedisResult scanResult = await DB.ExecuteAsync("SCAN", cursor, "MATCH", Options.KeyPattern, "COUNT", Options.ScanCountPerPage);
        cursor = ulong.Parse((string)scanResult[0]);
        var keys = scanResult[1];

        if (keys.Length == 0)
        {
            //Return empty list in case when scan did not find any keys
            return export;
        }
        
        for (int i = 0; i < keys.Length; i++)
            export.Add(new ExportValue(){k = (string)keys[i]});

        IBatch batch = DB.CreateBatch();
        foreach (var ex in export)
        {
            tasks.Add(batch.ExecuteAsync("TTL", ex.k).ContinueWith(x => ex.ttl = (long)x.Result));
            tasks.Add(batch.ExecuteAsync("DUMP", ex.k).ContinueWith(x => ex.v = (byte[])x.Result));
        }

        batch.Execute();
        await Task.WhenAll(tasks);
        tasks.Clear();
        return export;
    }
}