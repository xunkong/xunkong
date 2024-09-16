namespace Xunkong.Hoyolab.Avatar;

/// <summary>
/// 武器信息
/// </summary>
public class AvatarWeapon
{

    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 武器图标
    /// <see href="https://upload-bbs.mihoyo.com/game_record/genshin/equip/UI_EquipIcon_Sword_Narukami.png"/>
    /// </summary>
    [JsonPropertyName("icon")]
    public string Icon { get; set; }

    /// <summary>
    /// 武器类型，1单手，11双手，13长柄，10法器，12弓
    /// </summary>
    [JsonPropertyName("type")]
    public int Type { get; set; }

    /// <summary>
    /// 稀有度
    /// </summary>
    [JsonPropertyName("rarity")]
    public int Rarity { get; set; }

    /// <summary>
    /// 等级
    /// </summary>
    [JsonPropertyName("level")]
    public int Level { get; set; }

    /// <summary>
    /// 突破等级
    /// </summary>
    [JsonPropertyName("promote_level")]
    public int PromoteLevel { get; set; }

    /// <summary>
    /// 类型名称
    /// </summary>
    [JsonPropertyName("type_name")]
    public string TypeName { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [JsonPropertyName("desc")]
    public string Description { get; set; }

    /// <summary>
    /// 精炼等级
    /// </summary>
    [JsonPropertyName("affix_level")]
    public int AffixLevel { get; set; }

}
