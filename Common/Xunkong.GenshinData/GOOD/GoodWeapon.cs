namespace Xunkong.GenshinData.GOOD;

/// <summary>
/// 武器
/// </summary>
public class GoodWeapon
{

    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [JsonPropertyName("key")]
    public string Name { get; set; }

    /// <summary>
    /// 等级
    /// </summary>
    [JsonPropertyName("level")]
    public int Level { get; set; }

    /// <summary>
    /// 突破等级
    /// </summary>
    [JsonPropertyName("ascension")]
    public int PromoteLevel { get; set; }

    /// <summary>
    /// 精炼等级
    /// </summary>
    [JsonPropertyName("refinement")]
    public int AffixLevel { get; set; }

    /// <summary>
    /// 装备角色
    /// </summary>
    [JsonPropertyName("location")]
    public string EquippedCharacter { get; set; }

    /// <summary>
    /// 锁定
    /// </summary>
    [JsonPropertyName("lock")]
    public bool Lock { get; set; }

}
