using System.Text.Json.Serialization;

namespace Xunkong.SnapMetadata;

public class SnapGachaEventInfo
{

    public string Name { get; set; }
    public string Version { get; set; }
    public int Order { get; set; }
    public string Banner { get; set; }
    public string Banner2 { get; set; }
    public DateTimeOffset From { get; set; }
    public DateTimeOffset To { get; set; }
    public int Type { get; set; }
    public List<int> UpOrangeList { get; set; }
    public List<int> UpPurpleList { get; set; }

    [JsonIgnore]
    public int QueryType => Type == 400 ? 301 : Type;
}
