using System.ComponentModel;
using Newtonsoft.Json;

namespace RedEx;

public class ExportValue
{
    [JsonProperty(Order = -90)]
    public string k;
    [JsonProperty(Order = -80)]
    public byte[]? v;
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Order = -70), DefaultValue(-1)]
    public long? ttl;
}