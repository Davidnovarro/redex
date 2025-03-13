using System.Text;

namespace RedEx;

using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class MultiThreadFileReader : IDisposable
{
    public readonly ConcurrentQueue<string> Queue = new();
    private readonly CancellationToken _token;
    public readonly CancellationTokenSource CancellationTokenSource = new();
    private readonly StreamReader reader;
    public readonly Task task;
    
    private readonly int MaxQueueSize;

    public MultiThreadFileReader(string filePath, int maxQueueSize)
    {
        _token = CancellationTokenSource.Token;
        reader = new StreamReader(filePath, Encoding.UTF8, true, 1024 * 1024 * 16);
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
            
            var line = await reader.ReadLineAsync(_token);
            
            if(line == null)
                break;
            
            Queue.Enqueue(line);
        }
    }
    
    public void Dispose()
    {
        CancellationTokenSource.Dispose();
        reader.Dispose();
    }
}