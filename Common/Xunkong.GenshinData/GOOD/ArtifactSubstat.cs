namespace Xunkong.GenshinData.GOOD;

/// <summary>
/// 圣遗物副属性
/// </summary>
public class ArtifactSubstat
{
    /// <summary>
    /// 属性名
    /// </summary>
    [JsonPropertyName("key")]
    public string Key { get; set; }

    /// <summary>
    /// 属性值
    /// </summary>
    [JsonPropertyName("value")]
    public double Value { get; set; }

}
