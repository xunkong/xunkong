namespace Xunkong.Hoyolab.GameRecord;

/// <summary>
/// 尘歌壶
/// </summary>
public class PotHome
{

    /// <summary>
    /// 信任等阶
    /// </summary>
    [JsonPropertyName("level")]
    public int Level { get; set; }

    /// <summary>
    /// 历史访客数
    /// </summary>
    [JsonPropertyName("visit_num")]
    public int VisitNumber { get; set; }

    /// <summary>
    /// 最高洞天仙力
    /// </summary>
    [JsonPropertyName("comfort_num")]
    public int ComfortNumber { get; set; }

    /// <summary>
    /// 摆设数
    /// </summary>
    [JsonPropertyName("item_num")]
    public int ItemNumber { get; set; }

    /// <summary>
    /// 洞天名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 洞天图片
    /// <see href="https://upload-bbs.mihoyo.com/game_record/genshin/home/UI_HomeworldModule_1_Pic.png"/>
    /// </summary>
    [JsonPropertyName("icon")]
    public string Icon { get; set; }

    /// <summary>
    /// 仙力等级名称
    /// </summary>
    [JsonPropertyName("comfort_level_name")]
    public string ComfortLevelName { get; set; }

    /// <summary>
    /// 仙力等级图标
    /// <see href="https://upload-bbs.mihoyo.com/game_record/genshin/home/UI_Homeworld_Comfort_10.png"/>
    /// </summary>
    [JsonPropertyName("comfort_level_icon")]
    public string ComfortLevelIcon { get; set; }

}
