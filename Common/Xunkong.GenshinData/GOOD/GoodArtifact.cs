namespace Xunkong.GenshinData.GOOD;

/// <summary>
/// 圣遗物
/// </summary>
public class GoodArtifact
{

    /// <summary>
    /// 名称
    /// </summary>
    [JsonPropertyName("setKey")]
    public string Name { get; set; }

    /// <summary>
    /// 部位
    /// </summary>
    [JsonPropertyName("slotKey")]
    public string Location { get; set; }

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
    /// 主属性
    /// </summary>
    [JsonPropertyName("mainStatKey")]
    public string MainStatKey { get; set; }

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

    /// <summary>
    /// 副属性
    /// </summary>
    [JsonPropertyName("substats")]
    public List<ArtifactSubstat> Substats { get; set; }

}
