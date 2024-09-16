namespace Xunkong.GenshinData;

public class Cutscene
{
    public string Version { get; set; }

    public string Title { get; set; }

    public string Group { get; set; }

    public string Chapter { get; set; }

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Type { get; set; }

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Player { get; set; }

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long Size { get; set; }

    [JsonIgnore]
    public string SizeString => $"{(double)Size / (1 << 20):F2} MB";

    public string Poster { get; set; }

    public string Source { get; set; }

    public string? Comment { get; set; }

}
