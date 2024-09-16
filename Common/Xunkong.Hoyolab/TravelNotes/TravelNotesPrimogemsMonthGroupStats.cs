namespace Xunkong.Hoyolab.TravelNotes;

/// <summary>
/// 旅行记录原石获取的分组统计
/// </summary>
public class TravelNotesPrimogemsMonthGroupStats
{

    [JsonIgnore]
    public int Id { get; set; }

    public int Uid { get; set; }

    public int Year { get; set; }

    public int Month { get; set; }


    [JsonPropertyName("action_id")]
    public int ActionId { get; set; }


    [JsonPropertyName("action")]
    public string? ActionName { get; set; }


    [JsonPropertyName("num")]
    public int Number { get; set; }


    [JsonPropertyName("percent")]
    public int Percent { get; set; }

}
