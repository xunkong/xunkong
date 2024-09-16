namespace Xunkong.Hoyolab.TravelNotes;

/// <summary>
/// 旅行记录原石或摩拉获取记录
/// </summary>
public class TravelNotesAwardItem
{

    [JsonIgnore]
    public int Id { get; set; }

    public int Uid { get; set; }

    public int Year { get; set; }

    public int Month { get; set; }

    public TravelNotesAwardType Type { get; set; }


    [JsonPropertyName("action_id")]
    public int ActionId { get; set; }


    [JsonPropertyName("action")]
    public string ActionName { get; set; }

    /// <summary>
    /// 获取时间，UTC+8
    /// </summary>
    [JsonPropertyName("time"), JsonConverter(typeof(TravelNotesDateTimeJsonConverter))]
    public DateTime Time { get; set; }


    [JsonPropertyName("num")]
    public int Number { get; set; }

}
