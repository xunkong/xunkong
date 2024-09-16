namespace Xunkong.GenshinData.Text;

public class TextMapItem
{

    [JsonPropertyName("item_id"), JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int ItemId { get; set; }

    [JsonPropertyName("text_hash"), JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long TextHash { get; set; }

    [JsonPropertyName("chs")]
    public string CHS { get; set; }

    [JsonPropertyName("cht")]
    public string CHT { get; set; }

    [JsonPropertyName("de")]
    public string DE { get; set; }

    [JsonPropertyName("en")]
    public string EN { get; set; }

    [JsonPropertyName("es")]
    public string ES { get; set; }

    [JsonPropertyName("fr")]
    public string FR { get; set; }

    [JsonPropertyName("id")]
    public string ID { get; set; }

    [JsonPropertyName("it")]
    public string IT { get; set; }

    [JsonPropertyName("jp")]
    public string JP { get; set; }

    [JsonPropertyName("kr")]
    public string KR { get; set; }

    [JsonPropertyName("pt")]
    public string PT { get; set; }

    [JsonPropertyName("ru")]
    public string RU { get; set; }

    [JsonPropertyName("th")]
    public string TH { get; set; }

    [JsonPropertyName("tr")]
    public string TR { get; set; }

    [JsonPropertyName("vi")]
    public string VI { get; set; }

}