using System.Text.Json.Serialization;

namespace Xunkong.Desktop.Models;

public class CharacterInfoPage2_CharacterInfo
{

    public int Id { get; set; }

    public int Index { get; set; }

    public string Name { get; set; }

    public int Rarity { get; set; }

    public ElementType Element { get; set; }

    public string Icon { get; set; }

    public int Level { get; set; }

    public int Constellations { get; set; }

    public int Fetter { get; set; }

    public int SkillLevel_A { get; set; }

    public string SkillBuff_A_Icon { get; set; }

    [JsonIgnore]
    public int ActualSkillLevel_A => string.IsNullOrWhiteSpace(SkillBuff_A_Icon) ? SkillLevel_A : SkillLevel_A + 1;

    public int SkillLevel_E { get; set; }

    /// <summary>
    /// 增加技能等级的命座图片，未激活则为 null
    /// </summary>
    public string SkillBuff_E_Icon { get; set; }

    /// <summary>
    /// 计算命座加成后的技能等级
    /// </summary>
    [JsonIgnore]
    public int ActualSkillLevel_E => string.IsNullOrWhiteSpace(SkillBuff_E_Icon) ? SkillLevel_E : SkillLevel_E + 3;

    public int SkillLevel_Q { get; set; }

    public string SkillBuff_Q_Icon { get; set; }

    [JsonIgnore]
    public int ActualSkillLevel_Q => string.IsNullOrWhiteSpace(SkillBuff_Q_Icon) ? SkillLevel_Q : SkillLevel_Q + 3;

    public CharacterInfoPage2_WeaponInfo Weapon { get; set; }

    public List<CharacterInfoPage2_ReliquaryInfo> Reliquaries { get; set; }
}


public class CharacterInfoPage2_WeaponInfo
{

    public int Id { get; set; }

    public string Name { get; set; }

    public int Rarity { get; set; }

    public string Icon { get; set; }

    public int Type { get; set; }

    public int Level { get; set; }

    public int AffixLevel { get; set; }

}

public class CharacterInfoPage2_ReliquaryInfo
{

    public int Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 圣遗物图标
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// 圣遗物位置，1-5花羽沙杯冠
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// 稀有度
    /// </summary>
    public int Rarity { get; set; }

    /// <summary>
    /// 强化等级
    /// </summary>
    public int Level { get; set; }

}