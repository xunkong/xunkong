namespace Xunkong.Hoyolab.Avatar;

/// <summary>
/// 角色信息
/// </summary>
public class AvatarInfo
{

    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 元素类型
    /// </summary>
    [JsonPropertyName("element"), JsonConverter(typeof(JsonStringEnumConverter))]
    public ElementType Element { get; set; }

    /// <summary>
    /// 半身肖像
    /// <see href="https://upload-bbs.mihoyo.com/game_record/genshin/character_icon/UI_AvatarIcon_Ayaka.png"/>
    /// </summary>
    [JsonPropertyName("image")]
    public string Image { get; set; }

    /// <summary>
    /// 半身肖像（带有卡片边框）
    /// <see href="https://upload-bbs.mihoyo.com/game_record/genshin/character_card_icon/UI_AvatarIcon_Ayaka_Card.png"/>
    /// </summary>
    [JsonPropertyName("card_image")]
    public string CardImage { get; set; }

    /// <summary>
    /// 好感度
    /// </summary>
    [JsonPropertyName("fetter")]
    public int Fetter { get; set; }

    /// <summary>
    /// 等级
    /// </summary>
    [JsonPropertyName("level")]
    public int Level { get; set; }

    /// <summary>
    /// 稀有度
    /// </summary>
    [JsonPropertyName("rarity")]
    public int Rarity { get; set; }

    /// <summary>
    /// 已激活名座数
    /// </summary>
    [JsonPropertyName("actived_constellation_num")]
    public int ActivedConstellationNumber { get; set; }

}
