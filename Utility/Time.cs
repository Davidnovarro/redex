using System;
using System.Diagnostics;

namespace Party.Utility
{
    public static class Time
    {
        private static ITImeProvider Provider;
        
        static Time()
        {
            SetTimeProvider(new StopwatchTimeProvider());
        }

        public static void SetTimeProvider(ITImeProvider provider)
        {
            Provider = provider ?? new StopwatchTimeProvider();
        }

        // returns the clock time _in this system_
        public static double time => Provider.GetTime();

        public static long UtcNow => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();


        /// <summary>
        /// Time elapsed since UTC in seconds
        /// </summary>
        /// <param name="utcTime"></param>
        /// <returns></returns>
        public static double ElapsedSinceUTC(long utcTime)
        {
            return (UtcNow - utcTime) / 1000d;
        }

        public static double ElapsedSince(double timeStamp)
        {
            return time - timeStamp;
        }

        public static long ToUtc(this DateTime dt)
        {
            return new DateTimeOffset(dt).ToUnixTimeMilliseconds();
        }

        public static long ToUtc(int year, int month, int day, int hour, int minute, int second)
        {
            return new DateTimeOffset(new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc)).ToUnixTimeMilliseconds();
        }

        public static DateTime FromUtc(long utc)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(utc).ToUniversalTime().DateTime;
        }
    }
    
    public interface ITImeProvider
    {
        double GetTime();
    }
        
    public class StopwatchTimeProvider : ITImeProvider
    {
        private readonly Stopwatch stopwatch = new Stopwatch();

        public StopwatchTimeProvider()
        {
            stopwatch.Start();
        }
            
        public double GetTime()
        {
            return stopwatch.Elapsed.TotalSeconds;
        }
    }
}
