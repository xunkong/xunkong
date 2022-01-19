namespace Xunkong.Core.Hoyolab
{

    /// <summary>
    /// 探索统计信息
    /// </summary>
    public class PlayerStats
    {

        /// <summary>
        /// 活跃天数
        /// </summary>
        [JsonPropertyName("active_day_number")]
        public int ActiveDayNumber { get; set; }

        /// <summary>
        /// 成就达成数
        /// </summary>
        [JsonPropertyName("achievement_number")]
        public int AchievementNumber { get; set; }

        /// <summary>
        /// 风神瞳
        /// </summary>
        [JsonPropertyName("anemoculus_number")]
        public int AnemoculusNumber { get; set; }

        /// <summary>
        /// 岩神瞳
        /// </summary>
        [JsonPropertyName("geoculus_number")]
        public int GeoculusNumber { get; set; }

        /// <summary>
        /// 雷神瞳
        /// </summary>
        [JsonPropertyName("electroculus_number")]
        public int ElectroculusNumber { get; set; }

        /// <summary>
        /// 获得角色数
        /// </summary>
        [JsonPropertyName("avatar_number")]
        public int AvatarNumber { get; set; }

        /// <summary>
        /// 解锁传送点
        /// </summary>
        [JsonPropertyName("way_point_number")]
        public int WayPointNumber { get; set; }

        /// <summary>
        /// 解锁秘境
        /// </summary>
        [JsonPropertyName("domain_number")]
        public int DomainNumber { get; set; }

        /// <summary>
        /// 深境螺旋
        /// </summary>
        [JsonPropertyName("spiral_abyss")]
        public string SpiralAbyss { get; set; }

        /// <summary>
        /// 华丽宝箱
        /// </summary>
        [JsonPropertyName("luxurious_chest_number")]
        public int LuxuriousChestNumber { get; set; }

        /// <summary>
        /// 珍贵宝箱
        /// </summary>
        [JsonPropertyName("precious_chest_number")]
        public int PreciousChestNumber { get; set; }

        /// <summary>
        /// 精致宝箱
        /// </summary>
        [JsonPropertyName("exquisite_chest_number")]
        public int ExquisiteChestNumber { get; set; }

        /// <summary>
        /// 普通宝箱
        /// </summary>
        [JsonPropertyName("common_chest_number")]
        public int CommonChestNumber { get; set; }


        /// <summary>
        /// 奇馈宝箱
        /// </summary>
        [JsonPropertyName("magic_chest_number")]
        public int MagicChestNumber { get; set; }



    }
}
