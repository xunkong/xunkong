namespace Xunkong.Hoyolab.Avatar;


/// <summary>
/// 角色养成计算器
/// </summary>
public class AvatarCalculate
{
    /// <summary>
    /// 角色技能
    /// </summary>
    [JsonPropertyName("skill_list")]
    public List<AvatarSkill> Skills { get; set; }


    //[JsonPropertyName("weapon")]
    //public Weapon? Weapon { get; set; }


    //[JsonPropertyName("reliquary_list")]
    //public List<Reliquary>? ReliquaryList { get; set; }

}
