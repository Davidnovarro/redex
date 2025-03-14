using Party.Utility;
using ShellProgressBar;

namespace RedEx;

public class ProgressInfo : IDisposable
{
    public readonly long startUtc;
    
    public double ElapsedSeconds => Time.ElapsedSinceUTC(startUtc);
    public double TicksPerSecond => bar.CurrentTick <= 0 || ElapsedSeconds <= 0.0000001d ? 0 : bar.CurrentTick / ElapsedSeconds;
    public double EstimatedDuration => TicksPerSecond <= 0.0000001d ? 0 : bar.MaxTicks / TicksPerSecond;
    public string MessageFormat { get; set; }

    public readonly ProgressBar bar; 
    
    public ProgressInfo(int maxTicks, string messageFormat = "{0} of {1}")
    {
        MessageFormat = messageFormat;
        startUtc = Time.UtcNow;
        bar = new ProgressBar(maxTicks, null, new ProgressBarOptions
        {
            ShowEstimatedDuration = true,    
        });
    }

    public void Tick(int newTickCount)
    {
        bar.Tick(newTickCount, TimeSpan.FromSeconds(EstimatedDuration), GetMessage());
    }

    public string GetMessage()
    {
        return string.Format(MessageFormat, bar.CurrentTick, bar.MaxTicks);
    }

    public void Dispose()
    {
        bar.Dispose();
    }
}