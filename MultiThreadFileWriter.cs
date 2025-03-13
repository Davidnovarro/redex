using System.Text;

namespace RedEx;

using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class MultiThreadFileWriter : IDisposable
{
    public readonly ConcurrentQueue<string> Queue = new();
    public readonly CancellationTokenSource CancellationTokenSource = new();
    private readonly CancellationToken _token;
    private readonly StreamWriter writer;
    public readonly Task task;
    
    public MultiThreadFileWriter(string filePath)
    {
        _token = CancellationTokenSource.Token;
        writer = new StreamWriter(filePath, false, Encoding.UTF8, 1024 * 1024 * 16);
        // This is the task that will run
        // in the background and do the actual file writing
        task = Task.Run(WriteToFileAsync, _token);
    }

    public void WriteLine(string line)
    {
        Queue.Enqueue(line);
    }
    
    private async Task WriteToFileAsync()
    {
        while (true)
        {
            if (_token.IsCancellationRequested)
                return;
            
            while (Queue.TryDequeue(out string line))
            {
                await writer.WriteLineAsync(line);
            }
            
            // ReSharper disable once MethodSupportsCancellation
            await writer.FlushAsync();
            Thread.Sleep(1);
        }
    }

    public void Dispose()
    {
        CancellationTokenSource.Dispose();
        writer.Dispose();
    }
}