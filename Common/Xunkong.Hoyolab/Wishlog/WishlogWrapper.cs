namespace Xunkong.Hoyolab.Wishlog;

internal class WishlogWrapper
{
    [JsonPropertyName("page"), JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Page { get; set; }

    [JsonPropertyName("size"), JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Size { get; set; }

    [JsonPropertyName("total"), JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Total { get; set; }

    [JsonPropertyName("list")]
    public List<WishlogItem> List { get; set; }

    [JsonPropertyName("region")]
    public string Region { get; set; }
}
