namespace Xunkong.GenshinData.Text;

public class TextMap
{

    [JsonPropertyName("type")]
    public string Type { get; set; }


    [JsonPropertyName("version")]
    public string Version { get; set; }


    [JsonPropertyName("update_time")]
    public DateTimeOffset UpdateTime { get; set; }


    [JsonPropertyName("list")]
    public List<TextMapItem> List { get; set; }

}
