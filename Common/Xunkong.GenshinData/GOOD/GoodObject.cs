namespace Xunkong.GenshinData.GOOD;

/// <summary>
/// Genshin Open Object Description (GOOD)
/// </summary>
public class GoodObject
{
    /// <summary>
    /// GOOD
    /// </summary>
    [JsonPropertyName("format")]
    public string Format { get; set; } = "GOOD";

    /// <summary>
    /// 协议版本
    /// </summary>
    [JsonPropertyName("version")]
    public int Version { get; set; }

    /// <summary>
    /// 来源
    /// </summary>
    [JsonPropertyName("source")]
    public string Source { get; set; }

    /// <summary>
    /// 角色
    /// </summary>
    [JsonPropertyName("characters")]
    public List<GoodCharacter>? Characters { get; set; }

    /// <summary>
    /// 圣遗物
    /// </summary>
    [JsonPropertyName("artifacts")]
    public List<GoodArtifact>? Artifacts { get; set; }

    /// <summary>
    /// 武器
    /// </summary>
    [JsonPropertyName("weapons")]
    public List<GoodWeapon>? Weapons { get; set; }

    /// <summary>
    /// 材料
    /// </summary>
    [JsonPropertyName("materials")]
    public Dictionary<string, int>? Materials { get; set; }

}
