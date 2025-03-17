using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;

namespace RedEx;

using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class MultiThreadExportValueWriter : IDisposable
{
    public readonly ConcurrentQueue<ExportValue> Queue = new();
    public readonly CancellationTokenSource CancellationTokenSource = new();
    private readonly CancellationToken _token;
    private readonly StreamWriter writer;
    private readonly JsonWriter jsonWriter;

    public readonly Task task;
    
    public MultiThreadExportValueWriter(string filePath)
    {
        _token = CancellationTokenSource.Token;
        writer = new StreamWriter(filePath, false, Encoding.UTF8, 1024 * 1024 * 16);
        jsonWriter = new JsonTextWriter(writer);
        // This is the task that will run
        // in the background and do the actual file writing
        task = Task.Run(WriteToFileAsync, _token);
    }

    public void WriteLine(ExportValue ev)
    {
        Queue.Enqueue(ev);
    }
    
    private async Task WriteToFileAsync()
    {
        while (true)
        {
            if (_token.IsCancellationRequested)
                return;
            
            while (Queue.TryDequeue(out ExportValue ev))
            {
                Write(jsonWriter, ev);
                await writer.WriteAsync(writer.NewLine);
            }
            
            // ReSharper disable once MethodSupportsCancellation
            await jsonWriter.FlushAsync();
            // ReSharper disable once MethodSupportsCancellation
            await writer.FlushAsync();
            Thread.Sleep(1);
        }
    }

    private static void Write(JsonWriter writer, ExportValue ev)
    {
        writer.WriteStartObject();
        
        writer.WritePropertyName(nameof(ExportValue.k));
        writer.WriteValue(ev.k);

        if (ev.ttl.HasValue)
        {
            writer.WritePropertyName(nameof(ExportValue.ttl));
            writer.WriteValue(ev.ttl);    
        }
        
        writer.WritePropertyName(nameof(ExportValue.v));
        writer.WriteValue(ev.v);
        writer.WriteEndObject();
    }
    

    public void Dispose()
    {
        CancellationTokenSource.Dispose();
        writer.Dispose();
        ((IDisposable)jsonWriter).Dispose();
    }
}