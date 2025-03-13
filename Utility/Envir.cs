using Newtonsoft.Json;
using Party.Utility;
using System;

namespace Party.Shared.Utility
{
    public static class Envir
    {
        public static string GetStringOrDefault(string variable, string defaultValue)
        {
            var result = Environment.GetEnvironmentVariable(variable);
            return !string.IsNullOrEmpty(result) ? result : defaultValue;
        }
        
        public static bool GetBoolOrDefault(string variable, bool defaultValue = false)
        {
            var result = Environment.GetEnvironmentVariable(variable);
            if (result == null)
                return defaultValue;

            return result.ToLowerInvariant() switch
            {
                "true" or "yes" or "1" => true,
                "false" or "no" or "0" => false,
                _ => defaultValue
            };
        }
        
        public static long GetLongOrDefault(string variable, long defaultValue = 0)
        {
            var result = Environment.GetEnvironmentVariable(variable);
            if (result == null)
                return defaultValue;

            return long.TryParse(result, out var value) ? value : defaultValue;
        }
        
        
        public static int GetIntOrDefault(string variable, int defaultValue = 0)
        {
            var result = Environment.GetEnvironmentVariable(variable);
            if (result == null)
                return defaultValue;

            return int.TryParse(result, out var value) ? value : defaultValue;
        }
        
        public static float GetFloatOrDefault(string variable, float defaultValue = 0)
        {
            var result = Environment.GetEnvironmentVariable(variable);
            if (result == null)
                return defaultValue;

            return float.TryParse(result, out var value) ? value : defaultValue;
        }
        
        public static double GetDoubleOrDefault(string variable, double defaultValue = 0)
        {
            var result = Environment.GetEnvironmentVariable(variable);
            if (result == null)
                return defaultValue;

            return double.TryParse(result, out var value) ? value : defaultValue;
        }
    }
}