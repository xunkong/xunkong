namespace Xunkong.Core.TravelRecord
{
    /// <summary>
    /// 旅行记录原石获取每月统计项
    /// </summary>
    [Table("TravelTecord_GroupStats")]
    public class TravelRecordPrimogemsMonthGroupStats
    {

        [JsonIgnore]
        public int Id { get; set; }

        [JsonIgnore]
        public int TravelRecordMonthDataId { get; set; }

        public int Uid { get; set; }

        public int Year { get; set; }

        public int Month { get; set; }


        [JsonPropertyName("action_id")]
        public int ActionId { get; set; }


        [JsonPropertyName("action"), MaxLength(255)]
        public string? ActionName { get; set; }


        [JsonPropertyName("num")]
        public int Number { get; set; }


        [JsonPropertyName("percent")]
        public int Percent { get; set; }


    }
}
