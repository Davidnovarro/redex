using System.Collections.Concurrent;
using System.Diagnostics;

namespace Party.Utility
{
    public static class Profiler
    {
        private static readonly ConcurrentDictionary<string, Stopwatch> Stopwatches = new ConcurrentDictionary<string, Stopwatch>();

        public static bool Contains(string name)
        {
            return Stopwatches.ContainsKey(name);
        }

        public static void Start(string name)
        {
            Stopwatches[name] = new Stopwatch();
            Stopwatches[name].Start();
        }

        public static void Restart(string name)
        {
            if (Stopwatches.TryGetValue(name, out Stopwatch value))
                value.Restart();
        }


        public static void Stop(string name)
        {
            if (Stopwatches.TryGetValue(name, out Stopwatch value))
                value.Stop();
        }

        public static void Reset(string name)
        {
            if (Stopwatches.TryGetValue(name, out Stopwatch value))
                value.Reset();
        }

        /// <summary>
        /// Prints and returns TotalMilliseconds also destroys Stopwatch
        /// </summary>
        public static double End(string name)
        {
            if (!Stopwatches.TryGetValue(name, out Stopwatch value))
                return -1;
            Stop(name);

            var ms = value.Elapsed.TotalMilliseconds;

            PrintMilliseconds(name);
            Dispose(name);
            return ms;
        }

        public static void Toggle(string name)
        {
            if(!Stopwatches.ContainsKey(name))
                Start(name);
            else
                End(name);
        }

        public static void PrintMilliseconds(string name)
        {
            if (!Stopwatches.TryGetValue(name, out Stopwatch value))
                return;
            var ms = value.Elapsed.TotalMilliseconds;
            Console.WriteLine(name + " : " + System.Math.Round(ms, 2) + " ms");
        }

        public static void Dispose(string name)
        {
            if (!Stopwatches.ContainsKey(name))
                return;
            Stop(name);
            Stopwatches.TryRemove(name, out _);
        }
    }
}
