namespace Xunkong.GenshinData.Character;

/// <summary>
/// 角色天赋（技能）
/// </summary>
public class CharacterTalent
{

    public int TalentId { get; set; }

    /// <summary>
    /// 在角色天赋中的排序
    /// </summary>
    public int Order { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Icon { get; set; }

    public float CdTime { get; set; }

    /// <summary>
    /// 可积攒次数
    /// </summary>
    public int MaxChargeNumber { get; set; }

    /// <summary>
    /// 大招消耗的能量
    /// </summary>
    public float CostElementValue { get; set; }


    public List<CharacterTalentLevel> Levels { get; set; }

}


public class CharacterTalentLevel
{
    public int ProudSkillId { get; set; }

    public int ProudSkillGroupId { get; set; }

    public int Level { get; set; }

    public int CoinCost { get; set; }

    public List<PromotionCostItem> CostItems { get; set; }


    public List<CharacterTalentLevelParam> Params { get; set; }

}


public class CharacterTalentLevelParam
{
    /// <summary>
    /// 参数名称
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 参数数值
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// 原始参数名称
    /// </summary>
    public string Desc { get; set; }
}