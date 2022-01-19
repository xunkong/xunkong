namespace Xunkong.Core.Hoyolab
{
    public class AvatarReliquary
    {

        [JsonPropertyName("id")]
        public int Id { get; set; }


        [JsonPropertyName("name")]
        public string Name { get; set; }


        [JsonPropertyName("icon")]
        public string Icon { get; set; }

        /// <summary>
        /// 圣遗物位置，1-5花羽沙杯冠
        /// </summary>
        [JsonPropertyName("pos")]
        public int Position { get; set; }


        [JsonPropertyName("rarity")]
        public int Rarity { get; set; }


        [JsonPropertyName("level")]
        public int Level { get; set; }

        /// <summary>
        /// 套装及效果
        /// </summary>
        [JsonPropertyName("set")]
        public ReliquarySet ReliquarySet { get; set; }


        [JsonPropertyName("pos_name")]
        public string PositionName { get; set; }


    }

    public class ReliquarySet
    {

        [JsonPropertyName("id")]
        public int Id { get; set; }


        [JsonPropertyName("name")]
        public string Name { get; set; }


        [JsonPropertyName("affixes")]
        public List<ReliquaryAffix> Affixes { get; set; }

    }


    public class ReliquaryAffix
    {

        [JsonPropertyName("activation_number")]
        public int ActivationNumber { get; set; }


        [JsonPropertyName("effect")]
        public string Effect { get; set; }

    }


}