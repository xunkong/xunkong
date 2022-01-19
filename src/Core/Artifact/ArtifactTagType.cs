namespace Xunkong.Core.Artifact
{
    /// <summary>
    /// 圣遗物词条类型
    /// </summary>
    [Flags]
    public enum ArtifactTagType : uint
    {

        /// <summary>
        /// 无
        /// </summary>
        None = 0x_0000_0000,

        /// <summary>
        /// 生命值
        /// </summary>
        Health = 0x_0000_0001,

        /// <summary>
        /// 生命值%
        /// </summary>
        HealthPct = 0x_0000_0002,

        /// <summary>
        /// 攻击力
        /// </summary>
        Attack = 0x_0000_0004,

        /// <summary>
        /// 攻击力%
        /// </summary>
        AttackPct = 0x_0000_0008,

        /// <summary>
        /// 防御力
        /// </summary>
        Defence = 0x_0000_0010,

        /// <summary>
        /// 防御力%
        /// </summary>
        DefencePct = 0x_0000_0020,

        /// <summary>
        /// 元素精通
        /// </summary>
        ElementMaster = 0x_0000_0040,

        /// <summary>
        /// 元素充能效率%
        /// </summary>
        EnergyRecharge = 0x_0000_0080,

        /// <summary>
        /// 暴击率%
        /// </summary>
        CritialRate = 0x_0000_0100,

        /// <summary>
        /// 暴击伤害%
        /// </summary>
        CritialDamage = 0x_0000_0200,

        /// <summary>
        /// 治疗加成%
        /// </summary>
        HealingBonus = 0x_0000_0400,

        /// <summary>
        /// 物理伤害加成%
        /// </summary>
        PhysicalBonus = 0x_0000_0800,

        /// <summary>
        /// 火元素伤害加成%
        /// </summary>
        PyroBonus = 0x_0000_1000,

        /// <summary>
        /// 水元素伤害加成%
        /// </summary>
        HydroBonus = 0x_0000_2000,

        /// <summary>
        /// 风元素伤害加成%
        /// </summary>
        AnemoBonus = 0x_0000_4000,

        /// <summary>
        /// 雷元素伤害加成%
        /// </summary>
        EletroBonus = 0x_0000_8000,

        /// <summary>
        /// 草元素伤害加成%
        /// </summary>
        DendroBonus = 0x_0001_0000,

        /// <summary>
        /// 冰元素伤害加成%
        /// </summary>
        CryoBonus = 0x_0002_0000,

        /// <summary>
        /// 岩元素伤害加成%
        /// </summary>
        GeoBonus = 0x_0004_0000,

    }
}
