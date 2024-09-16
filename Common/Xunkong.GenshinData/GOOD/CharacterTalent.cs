namespace Xunkong.GenshinData.GOOD;

/// <summary>
/// 角色天赋等级，不包含命座提升
/// </summary>
public class CharacterTalent
{
    /// <summary>
    /// 普通攻击
    /// </summary>
    [JsonPropertyName("auto")]
    public int Auto { get; set; }

    /// <summary>
    /// 元素战技
    /// </summary>
    [JsonPropertyName("skill")]
    public int Skill { get; set; }

    /// <summary>
    /// 元素爆发
    /// </summary>
    [JsonPropertyName("burst")]
    public int Burst { get; set; }
}