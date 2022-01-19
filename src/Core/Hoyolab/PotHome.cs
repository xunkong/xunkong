namespace Xunkong.Core.Hoyolab
{

    /// <summary>
    /// 尘歌壶
    /// </summary>
    public class PotHome
    {


        [JsonPropertyName("level")]
        public int Level { get; set; }

        /// <summary>
        /// 访客数
        /// </summary>
        [JsonPropertyName("visit_num")]
        public int VisitNumber { get; set; }

        /// <summary>
        /// 洞天仙力
        /// </summary>
        [JsonPropertyName("comfort_num")]
        public int ComfortNumber { get; set; }

        /// <summary>
        /// 摆设数
        /// </summary>
        [JsonPropertyName("item_num")]
        public int ItemNumber { get; set; }


        [JsonPropertyName("name")]
        public string Name { get; set; }


        [JsonPropertyName("icon")]
        public string Icon { get; set; }

        /// <summary>
        /// 仙力等级名称
        /// </summary>
        [JsonPropertyName("comfort_level_name")]
        public string ComfortLevelName { get; set; }


        [JsonPropertyName("comfort_level_icon")]
        public string ComfortLevelIcon { get; set; }




    }
}
