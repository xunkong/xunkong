namespace Xunkong.Core.SpiralAbyss
{
    /// <summary>
    /// 深境螺旋最值统计
    /// </summary>
    [Table("SpiralAbyss_Ranks")]
    [Index(nameof(Type))]
    public class SpiralAbyssRank
    {

        [JsonIgnore]
        public int Id { get; set; }


        public SpiralAbyssRankType Type { get; set; }


        [JsonPropertyName("avatar_id")]
        public int AvatarId { get; set; }


        [JsonPropertyName("avatar_icon")]
        public string AvatarIcon { get; set; }


        [JsonPropertyName("value")]
        public int Value { get; set; }


        [JsonPropertyName("rarity")]
        public int Rarity { get; set; }


    }
}
