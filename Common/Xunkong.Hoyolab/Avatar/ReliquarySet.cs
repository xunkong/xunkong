namespace Xunkong.Hoyolab.Avatar;

/// <summary>
/// 圣遗物套装
/// </summary>
public class ReliquarySet
{

    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// 套装名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 圣遗物套装效果
    /// </summary>
    [JsonPropertyName("affixes")]
    public List<ReliquaryAffix> Affixes { get; set; }

}
