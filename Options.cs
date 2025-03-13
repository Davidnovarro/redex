using System.Runtime.InteropServices;
using CommandLine;
using Party.Shared.Utility;

namespace RedEx;

public abstract class OptionsBase
{
    [Option(shortName:'e',longName:"endpoint", Required = false, HelpText = "(Default \"127.0.0.1:6379\") Redis endpoint to connect to.", Default = null)]
    public string Endpoint { get; set; }
    
    [Option(longName: "db", Required = false, HelpText = "Redis DB Index", Default = -1)]
    public int DB { get; set; }
    
    [Option(shortName:'f',longName:"file", Required = true, HelpText = "(Default \"/redis_export.json\") File Path", Default = null)]
    public string FilePath { get; set; }

    public virtual void Init()
    {
        if(string.IsNullOrEmpty(Endpoint))
            Endpoint = Envir.GetStringOrDefault("REDIS_ENDPOINT", RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "127.0.0.1:6379" : "redis-cluster:6379");

        if (DB < 0)
            DB = Envir.GetIntOrDefault("REDIS_DB", -1);
        
        if(string.IsNullOrEmpty(FilePath))
            FilePath = Envir.GetStringOrDefault("REDEX_FILE_PATH", "/redis_export.json");
    }
}

public class ExportOptions : OptionsBase
{
    [Option(longName: "key-pattern", Required = false, HelpText = "(Default \"*\") Redis KEY pattern to use for scanning keys")]
    public string KeyPattern { get; set; }
    
    [Option(longName: "scan-count", Required = false, HelpText = "(Default: 5000) The amount of keys to scan per request")]
    public long ScanCountPerPage { get; set; }

    public override void Init()
    {
        base.Init();
        
        if(string.IsNullOrEmpty(KeyPattern))
            KeyPattern = Envir.GetStringOrDefault("REDIS_SCAN_KEY_PATTERN", "*");

        if (DB < 0)
            DB = Envir.GetIntOrDefault("REDIS_DB", -1);
        
        if(string.IsNullOrEmpty(FilePath))
            FilePath = Envir.GetStringOrDefault("REDEX_FILE_PATH", "/redis_export.json");

        if (ScanCountPerPage <= 0)
            ScanCountPerPage = Envir.GetLongOrDefault("REDIS_SCAN_COUNT_PER_PAGE", 5000);
    }
}

public class ImportOptions : OptionsBase
{
    [Option(longName: "batch-count", Required = false, HelpText = "(Default: 1000) The amount of keys to import per batch request")]
    public int BatchCount { get; set; }

    [Option(longName: "replace", Required = false, HelpText = "Overwrite keys in case if they already exist", Default = true)]
    public bool ReplaceKeys { get; set; } = true;
    
    public override void Init()
    {
        base.Init();
        if (BatchCount <= 0)
            BatchCount = Envir.GetIntOrDefault("REDIS_BATCH_COUNT", 1000);
    }
}