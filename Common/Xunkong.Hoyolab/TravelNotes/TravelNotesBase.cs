namespace Xunkong.Hoyolab.TravelNotes;

public abstract class TravelNotesBase
{

    [JsonPropertyName("uid")]
    public int Uid { get; set; }


    [JsonPropertyName("region"), JsonConverter(typeof(JsonStringEnumConverter))]
    public RegionType Region { get; set; }

    /// <summary>
    /// 米游社 ID
    /// </summary>
    [JsonPropertyName("account_id")]
    public int AccountId { get; set; }


    [JsonPropertyName("nickname")]
    public string Nickname { get; set; }

    /// <summary>
    /// 当前日期
    /// </summary>
    [JsonPropertyName("date"), JsonConverter(typeof(TravelNotesDateJsonConverter))]
    public DateTime Date { get; set; }

    /// <summary>
    /// 当前月
    /// </summary>
    [JsonPropertyName("month")]
    public int CurrentMonth { get; set; }

    /// <summary>
    /// 可查询月份
    /// </summary>
    [JsonPropertyName("optional_month")]
    public List<int> OptionalMonth { get; set; }

    /// <summary>
    /// 获取的数据所在的月份
    /// </summary>
    [JsonPropertyName("data_month")]
    public int DataMonth { get; set; }

}
