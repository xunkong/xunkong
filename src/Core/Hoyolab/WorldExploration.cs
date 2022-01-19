namespace Xunkong.Core.Hoyolab
{

    /// <summary>
    /// 世界探索信息
    /// </summary>
    public class WorldExploration
    {


        [JsonPropertyName("level")]
        public int Level { get; set; }


        /// <summary>
        /// Maxmium is 1000
        /// </summary>
        [JsonPropertyName("exploration_percentage")]
        public int ExplorationPercentage { get; set; }


        [JsonPropertyName("icon")]
        public string Icon { get; set; }


        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// 地区探索奖励类型（声望，贡品）
        /// </summary>
        [JsonPropertyName("type"), JsonConverter(typeof(JsonStringEnumConverter))]
        public WorldExplorationRewardType Type { get; set; }

        /// <summary>
        /// 地区贡品等级
        /// </summary>
        [JsonPropertyName("offerings")]
        public List<WorldExplorationOffering> Offerings { get; set; }


        [JsonPropertyName("id")]
        public int Id { get; set; }


    }


    /// <summary>
    /// 世界探索奖励类型
    /// </summary>
    public enum WorldExplorationRewardType
    {
        /// <summary>
        /// 声望
        /// </summary>
        Reputation,

        /// <summary>
        /// 贡品
        /// </summary>
        Offering,
    }


    /// <summary>
    /// 贡品类型奖励信息s
    /// </summary>
    public class WorldExplorationOffering
    {

        [JsonPropertyName("name")]
        public string Name { get; set; }


        [JsonPropertyName("level")]
        public int Level { get; set; }

    }
}
