namespace Xunkong.Core.Hoyolab
{
    public class AvatarWeapon
    {

        [JsonPropertyName("id")]
        public int Id { get; set; }


        [JsonPropertyName("name")]
        public string Name { get; set; }


        [JsonPropertyName("icon")]
        public string Icon { get; set; }


        /// <summary>
        /// 武器类型，1单手，11双手，13长柄，10法器，12弓
        /// </summary>
        [JsonPropertyName("type")]
        public int Type { get; set; }


        [JsonPropertyName("rarity")]
        public int Rarity { get; set; }


        [JsonPropertyName("level")]
        public int Level { get; set; }


        /// <summary>
        /// 突破等级
        /// </summary>
        [JsonPropertyName("promote_level")]
        public int PromoteLevel { get; set; }


        [JsonPropertyName("type_name")]
        public string TypeName { get; set; }


        [JsonPropertyName("desc")]
        public string Description { get; set; }


        /// <summary>
        /// 精炼等级
        /// </summary>
        [JsonPropertyName("affix_level")]
        public int AffixLevel { get; set; }

    }
}