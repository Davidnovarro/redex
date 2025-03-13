namespace RedEx;

public static class RedisExportType
{
    public const string String = "string";
    public const string Hash = "hash";
    public const string List = "list";
    public const string Set = "set";
    public const string SortedSet = "zset";
    public const string Stream = "stream";
    public const string Json = "ReJSON-RL";
    public const string TimeSeries = "TSDB-TYPE";
    
    public static readonly RType[] All =
    [
        RType.Hash, 
        RType.String, 
        RType.List, 
        RType.Set, 
        RType.SortedSet, 
        RType.Stream,
        RType.Json,
        RType.TimeSeries
    ];

    public static string ToStringType(this RType type)
    {
        return type switch
        {
            RType.String => String,
            RType.List => List,
            RType.Set => Set,
            RType.SortedSet => SortedSet,
            RType.Hash => Hash,
            RType.Stream => Stream,
            RType.Json => Json,
            RType.TimeSeries => TimeSeries,
            _ => throw new FormatException($"Invalid redis type: {type}")
        };
    }
    
    public static RType ToRedisType(string type)
    {
        return type switch
        {
            String => RType.String,
            List => RType.List,
            Set => RType.Set,
            SortedSet => RType.SortedSet,
            Hash => RType.Hash,
            Stream => RType.Stream,
            Json => RType.Json,
            TimeSeries => RType.TimeSeries,
            _ => throw new FormatException($"Invalid redis type: {type}")
        };
    }
}

public enum RType
{
     None = 0,
     String = 1,
     List = 2,
     Set = 3,
     SortedSet = 4,
     Hash = 5,
     Stream = 6,
     Json = 7,
     TimeSeries = 8
}