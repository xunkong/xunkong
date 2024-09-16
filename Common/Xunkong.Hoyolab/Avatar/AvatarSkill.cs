namespace Xunkong.Hoyolab.Avatar;

/// <summary>
/// 角色技能
/// </summary>
public class AvatarSkill
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("group_id")]
    public int GroupId { get; set; }

    /// <summary>
    /// 技能名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 技能图标
    /// <see href="icon=https://uploadstatic.mihoyo.com/hk4e/e20200928calculate/skill_icon_ud09dc/8de9cdec361d437f820aeff8a39566aa.png"/>
    /// </summary>
    [JsonPropertyName("icon")]
    public string Icon { get; set; }

    /// <summary>
    /// 当前等级（不计入名座加成）
    /// </summary>
    [JsonPropertyName("level_current")]
    public int CurrentLevel { get; set; }

    /// <summary>
    /// 最大等级（可升级为10，不可升级为1，不计入名座加成）
    /// </summary>
    [JsonPropertyName("max_level")]
    public int MaxLevel { get; set; }

}