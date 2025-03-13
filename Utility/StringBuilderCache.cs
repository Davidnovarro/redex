using System;
using System.Text;

namespace Party.Shared.Utility
{
    /// <summary>Provide a cached reusable instance of stringbuilder per thread.</summary>
    public static class StringBuilderCache
    {
        // The value 360 was chosen in discussion with performance experts as a compromise between using
        // as little memory per thread as possible and still covering a large part of short-lived
        // StringBuilder creations on the startup path of VS designers.
        internal const int MAX_BUILDER_SIZE = 16384;
        private const int DEFAULT_CAPACITY = 32; // == StringBuilder.DefaultCapacity

        [ThreadStatic]
        private static StringBuilder t_cachedInstance;

        /// <summary>Get a StringBuilder for the specified capacity.</summary>
        /// <remarks>If a StringBuilder of an appropriate size is cached, it will be returned and the cache emptied.</remarks>
        public static StringBuilder Acquire(int capacity = DEFAULT_CAPACITY)
        {
            if (capacity <= MAX_BUILDER_SIZE)
            {
                StringBuilder sb = t_cachedInstance;
                if (sb != null)
                {
                    // Avoid stringbuilder block fragmentation by getting a new StringBuilder
                    // when the requested size is larger than the current capacity
                    if (capacity <= sb.Capacity)
                    {
                        t_cachedInstance = null;
                        sb.Clear();
                        return sb;
                    }
                }
            }

            return new StringBuilder(capacity);
        }

        /// <summary>Place the specified builder in the cache if it is not too big.</summary>
        public static void Release(this StringBuilder sb)
        {
            if(sb == null)
                return;
            
            if (sb.Capacity <= MAX_BUILDER_SIZE)
            {
                t_cachedInstance = sb;
            }
        }

        /// <summary>ToString() the stringbuilder, Release it to the cache, and return the resulting string.</summary>
        public static string ToStringRelease(this StringBuilder sb)
        {
            string result = sb.ToString();
            Release(sb);
            return result;
        }
    }
}