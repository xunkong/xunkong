namespace Xunkong.Hoyolab.GameRecord;

/// <summary>
/// 世界探索信息
/// </summary>
public class WorldExploration
{

    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// 地区名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 探索等级
    /// </summary>
    [JsonPropertyName("level")]
    public int Level { get; set; }

    /// <summary>
    /// 探索度，满值 1000
    /// </summary>
    [JsonPropertyName("exploration_percentage")]
    public int ExplorationPercentage { get; set; }

    public string ExplorationString => ((double)ExplorationPercentage / 1000).ToString("P1");

    /// <summary>
    /// 地区卡片背景图
    /// <see href="https://upload-bbs.mihoyo.com/game_record/genshin/city_icon/UI_ChapterBackground_Liyue.png" />
    /// </summary>
    [JsonPropertyName("background_image")]
    public string BackgroundImage { get; set; }

    /// <summary>
    /// 地区封面背景图
    /// <see href="https://upload-bbs.mihoyo.com/game_record/genshin/city_icon/UI_ChapterCover_Liyue.png"/>
    /// </summary>
    [JsonPropertyName("cover")]
    public string Cover { get; set; }

    /// <summary>
    /// 地区图标（白色）
    /// <see href="https://upload-bbs.mihoyo.com/game_record/genshin/city_icon/UI_ChapterIcon_Liyue.png"/>
    /// </summary>
    [JsonPropertyName("icon")]
    public string Icon { get; set; }

    /// <summary>
    /// 地区图标（黑色）
    /// <see href="https://upload-bbs.mihoyo.com/game_record/genshin/city_icon/UI_ChapterInnerIcon_Liyue.png"/>
    /// </summary>
    [JsonPropertyName("inner_icon")]
    public string InnerIcon { get; set; }

    /// <summary>
    /// 米游社地图
    /// <see href="https://webstatic.mihoyo.com/ys/app/interactive-map/index.html?bbs_presentation_style=no_header\u0026_markerFps=24\u0026lang=zh-cn#/map/2?\u0026center=2602.08,-1612.55\u0026zoom=-1.00"/>
    /// </summary>
    [JsonPropertyName("map_url")]
    public string MapUrl { get; set; }


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


}
