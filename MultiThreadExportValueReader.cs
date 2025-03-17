using System.Text;
using Newtonsoft.Json;

namespace RedEx;

using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class MultiThreadExportValueReader : IDisposable
{
    public readonly ConcurrentQueue<ExportValue> Queue = new();
    private readonly CancellationToken _token;
    public readonly CancellationTokenSource CancellationTokenSource = new();
    private readonly StreamReader reader;
    private readonly JsonReader jsonReader;
    private readonly JsonSerializer jsonSerializer = new();

    public readonly Task task;
    
    private readonly int MaxQueueSize;
    
    public MultiThreadExportValueReader(string filePath, int maxQueueSize)
    {
        _token = CancellationTokenSource.Token;
        reader = new StreamReader(filePath, Encoding.UTF8, true, 1024 * 1024 * 16);
        jsonReader = new JsonTextReader(reader)
        {
            SupportMultipleContent = true,
        };
        MaxQueueSize = maxQueueSize;
        task = Task.Run(ReadFromFileAsync, _token);
    }

    private async Task ReadFromFileAsync()
    {
        while (true)
        {
            if (_token.IsCancellationRequested)
                return;

            while (Queue.Count >= MaxQueueSize)
                Thread.Sleep(1);

            if (!Read(jsonReader, out var ev))
                break;
            
            Queue.Enqueue(ev);
        }
    }

    private static bool Read(JsonReader reader, out ExportValue? value)
    {
        if (!reader.Read() || reader.TokenType != JsonToken.StartObject)
        {
            value = null;
            return false;
        }
        
        value = new ExportValue();

        // Assuming the reader is positioned at StartObject.
        do
        {
            if (reader.TokenType != JsonToken.PropertyName)
                continue;
            
            string propName = (string)reader.Value;

            switch (propName)
            {
                case nameof(ExportValue.k): value.k = reader.ReadAsString(); break;
                case nameof(ExportValue.v): value.v = reader.ReadAsBytes(); break;
                case nameof(ExportValue.ttl):
                {
                    reader.Read();
                    value.ttl = (long?)reader.Value;
                    break;
                }
                default: reader.Skip(); break;
            }
        } while (reader.Read() && reader.TokenType != JsonToken.EndObject);

        return true;
    }
    
    public void Dispose()
    {
        CancellationTokenSource.Dispose();
        reader.Dispose();
    }
}