using System.ComponentModel;

namespace Xunkong.Core;

/// <summary>
/// 武器类型
/// </summary>
public enum WeaponType
{

    /// <summary>
    /// 未知
    /// </summary>
    [Description("未知")]
    None = 0,

    /// <summary>
    /// 单手剑
    /// </summary>
    [Description("单手剑")]
    Sword = 1,

    /// <summary>
    /// 双手剑
    /// </summary>
    [Description("双手剑")]
    Claymore = 2,

    /// <summary>
    /// 长柄武器
    /// </summary>
    [Description("长柄武器")]
    Polearm = 4,

    /// <summary>
    /// 法器
    /// </summary>
    [Description("法器")]
    Catalyst = 8,

    /// <summary>
    /// 弓箭
    /// </summary>
    [Description("弓箭")]
    Bow = 16,

}
