namespace Xunkong.Core.Hoyolab
{

    /// <summary>
    /// 玩家数据总览
    /// </summary>
    public class PlayerSummaryInfo
    {
        /// <summary>
        /// 角色信息
        /// </summary>
        [JsonPropertyName("avatars")]
        public List<AvatarInfo> AvatarInfos { get; set; }

        /// <summary>
        /// 冒险统计
        /// </summary>
        [JsonPropertyName("stats")]
        public PlayerStats PlayerStat { get; set; }

        /// <summary>
        /// 世界探索
        /// </summary>
        [JsonPropertyName("world_explorations")]
        public List<WorldExploration> WorldExplorations { get; set; }

        /// <summary>
        /// 尘歌壶
        /// </summary>
        [JsonPropertyName("homes")]
        public List<PotHome> PotHomes { get; set; }
    }
}
