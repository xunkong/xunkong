using System.ComponentModel;

namespace Xunkong.Hoyolab.GameRecord;

/// <summary>
/// 冒险统计
/// </summary>
public class PlayerRiskStats
{

    /// <summary>
    /// 活跃天数
    /// </summary>
    [JsonPropertyName("active_day_number")]
    [Description("活跃天数")]
    public int ActiveDayNumber { get; set; }

    /// <summary>
    /// 成就达成数
    /// </summary>
    [JsonPropertyName("achievement_number")]
    [Description("成就达成数")]
    public int AchievementNumber { get; set; }

    /// <summary>
    /// 获得角色数
    /// </summary>
    [JsonPropertyName("avatar_number")]
    [Description("获得角色数")]
    public int AvatarNumber { get; set; }

    /// <summary>
    /// 解锁传送点
    /// </summary>
    [JsonPropertyName("way_point_number")]
    [Description("解锁传送点")]
    public int WayPointNumber { get; set; }

    /// <summary>
    /// 风神瞳
    /// </summary>
    [JsonPropertyName("anemoculus_number")]
    [Description("风神瞳")]
    public int AnemoculusNumber { get; set; }

    /// <summary>
    /// 岩神瞳
    /// </summary>
    [JsonPropertyName("geoculus_number")]
    [Description("岩神瞳")]
    public int GeoculusNumber { get; set; }

    /// <summary>
    /// 雷神瞳
    /// </summary>
    [JsonPropertyName("electroculus_number")]
    [Description("雷神瞳")]
    public int ElectroculusNumber { get; set; }

    /// <summary>
    /// 草神瞳
    /// </summary>
    [JsonPropertyName("dendroculus_number")]
    [Description("草神瞳")]
    public int DendroculusNumber { get; set; }

    /// <summary>
    /// 水神瞳
    /// </summary>
    [JsonPropertyName("hydroculus_number")]
    [Description("水神瞳")]
    public int HydroculusNumber { get; set; }

    /// <summary>
    /// 解锁秘境
    /// </summary>
    [JsonPropertyName("domain_number")]
    [Description("解锁秘境")]
    public int DomainNumber { get; set; }

    /// <summary>
    /// 深境螺旋
    /// </summary>
    [JsonPropertyName("spiral_abyss")]
    [Description("深境螺旋")]
    public string SpiralAbyss { get; set; }

    /// <summary>
    /// 华丽宝箱
    /// </summary>
    [JsonPropertyName("luxurious_chest_number")]
    [Description("华丽宝箱")]
    public int LuxuriousChestNumber { get; set; }

    /// <summary>
    /// 珍贵宝箱
    /// </summary>
    [JsonPropertyName("precious_chest_number")]
    [Description("珍贵宝箱")]
    public int PreciousChestNumber { get; set; }

    /// <summary>
    /// 精致宝箱
    /// </summary>
    [JsonPropertyName("exquisite_chest_number")]
    [Description("精致宝箱")]
    public int ExquisiteChestNumber { get; set; }

    /// <summary>
    /// 普通宝箱
    /// </summary>
    [JsonPropertyName("common_chest_number")]
    [Description("普通宝箱")]
    public int CommonChestNumber { get; set; }

    /// <summary>
    /// 奇馈宝箱
    /// </summary>
    [JsonPropertyName("magic_chest_number")]
    [Description("奇馈宝箱")]
    public int MagicChestNumber { get; set; }

}
