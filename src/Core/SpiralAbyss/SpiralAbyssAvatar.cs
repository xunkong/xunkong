namespace Xunkong.Core.SpiralAbyss
{
    /// <summary>
    /// 深境螺旋角色
    /// </summary>
    [Table("SpiralAbyss_Avatars")]
    [Index(nameof(AvatarId))]
    [Index(nameof(Level))]
    [Index(nameof(Rarity))]
    public class SpiralAbyssAvatar
    {

        [JsonIgnore]
        public int Id { get; set; }

        [JsonIgnore]
        public int SpiralAbyssBattleId { get; set; }


        [JsonPropertyName("id")]
        public int AvatarId { get; set; }


        [JsonPropertyName("icon")]
        public string Icon { get; set; }


        [JsonPropertyName("level")]
        public int Level { get; set; }


        [JsonPropertyName("rarity")]
        public int Rarity { get; set; }


    }
}
