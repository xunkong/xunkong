namespace Xunkong.Hoyolab.Avatar;

/// <summary>
/// 圣遗物
/// </summary>
public class AvatarReliquary
{

    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 圣遗物图标
    /// <see href="https://upload-bbs.mihoyo.com/game_record/genshin/equip/UI_RelicIcon_14001_4.png"/>
    /// </summary>
    [JsonPropertyName("icon")]
    public string Icon { get; set; }

    /// <summary>
    /// 圣遗物位置，1-5花羽沙杯冠
    /// </summary>
    [JsonPropertyName("pos")]
    public int Position { get; set; }

    /// <summary>
    /// 稀有度
    /// </summary>
    [JsonPropertyName("rarity")]
    public int Rarity { get; set; }

    /// <summary>
    /// 强化等级
    /// </summary>
    [JsonPropertyName("level")]
    public int Level { get; set; }

    /// <summary>
    /// 套装及效果
    /// </summary>
    [JsonPropertyName("set")]
    public ReliquarySet ReliquarySet { get; set; }

    /// <summary>
    /// 位置名称（生之花）
    /// </summary>
    [JsonPropertyName("pos_name")]
    public string PositionName { get; set; }


}
