namespace Xunkong.Core;

/// <summary>
/// 属性类型
/// </summary>
public class PropertyType
{

    /// <summary>
    /// 基础生命值
    /// </summary>
    public const string BaseHp = "FIGHT_PROP_BASE_HP";

    /// <summary>
    /// 基础攻击力
    /// </summary>
    public const string BaseAttack = "FIGHT_PROP_BASE_ATTACK";

    /// <summary>
    /// 基础防御力
    /// </summary>
    public const string BaseDefense = "FIGHT_PROP_BASE_DEFENSE";

    /// <summary>
    /// 百分比生命值
    /// </summary>
    public const string HpPercent = "FIGHT_PROP_HP_PERCENT";

    /// <summary>
    /// 治疗加成
    /// </summary>
    public const string HealthAdd = "FIGHT_PROP_HEAL_ADD";

    /// <summary>
    /// 百分比攻击力
    /// </summary>
    public const string AttackPercent = "FIGHT_PROP_ATTACK_PERCENT";

    /// <summary>
    /// 百分比防御力
    /// </summary>
    public const string DefensePercent = "FIGHT_PROP_DEFENSE_PERCENT";

    /// <summary>
    /// 元素精通
    /// </summary>
    public const string ElementMastery = "FIGHT_PROP_ELEMENT_MASTERY";

    /// <summary>
    /// 元素充能效率
    /// </summary>
    public const string ChargeEfficiency = "FIGHT_PROP_CHARGE_EFFICIENCY";

    /// <summary>
    /// 暴击率
    /// </summary>
    public const string Critical = "FIGHT_PROP_CRITICAL";

    /// <summary>
    /// 暴击伤害
    /// </summary>
    public const string CriticalHurt = "FIGHT_PROP_CRITICAL_HURT";

    /// <summary>
    /// 物理伤害加成
    /// </summary>
    public const string PhysicalHurt = "FIGHT_PROP_PHYSICAL_ADD_HURT";

    /// <summary>
    /// 火元素伤害加成
    /// </summary>
    public const string FireHurt = "FIGHT_PROP_FIRE_ADD_HURT";

    /// <summary>
    /// 水元素伤害加成
    /// </summary>
    public const string WaterHurt = "FIGHT_PROP_WATER_ADD_HURT";

    /// <summary>
    /// 风元素伤害加成
    /// </summary>
    public const string WindHurt = "FIGHT_PROP_WIND_ADD_HURT";

    /// <summary>
    /// 雷元素伤害加成
    /// </summary>
    public const string ElectroHurt = "FIGHT_PROP_ELEC_ADD_HURT";

    /// <summary>
    /// 草元素伤害加成
    /// </summary>
    public const string GrassHurt = "FIGHT_PROP_GRASS_ADD_HURT";

    /// <summary>
    /// 冰元素伤害加成
    /// </summary>
    public const string IceHurt = "FIGHT_PROP_ICE_ADD_HURT";

    /// <summary>
    /// 岩元素伤害加成
    /// </summary>
    public const string RockHurt = "FIGHT_PROP_ROCK_ADD_HURT";




    public static string GetDescription(string? type) => type switch
    {
        BaseHp => "基础生命值",
        BaseAttack => "基础攻击力",
        BaseDefense => "基础防御力",
        HpPercent => "百分比生命值",
        HealthAdd => "治疗加成",
        AttackPercent => "百分比攻击力",
        DefensePercent => "百分比防御力",
        ElementMastery => "元素精通",
        ChargeEfficiency => "元素充能效率",
        Critical => "暴击率",
        CriticalHurt => "暴击伤害",
        PhysicalHurt => "物理伤害加成",
        FireHurt => "火元素伤害加成",
        WaterHurt => "水元素伤害加成",
        WindHurt => "风元素伤害加成",
        ElectroHurt => "雷元素伤害加成",
        GrassHurt => "草元素伤害加成",
        IceHurt => "冰元素伤害加成",
        RockHurt => "岩元素伤害加成",
        _ => "",
    };













}
