namespace Xunkong.GenshinData.GOOD;

/// <summary>
/// 角色
/// </summary>
public class GoodCharacter
{
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
    /// 命座
    /// </summary>
    [JsonPropertyName("constellation")]
    public int Constellation { get; set; }

    /// <summary>
    /// 突破等级
    /// </summary>
    [JsonPropertyName("ascension")]
    public int PromoteLevel { get; set; }

    /// <summary>
    /// 天赋等级，不包含命座提升
    /// </summary>
    [JsonPropertyName("talent")]
    public CharacterTalent Talent { get; set; }

}
