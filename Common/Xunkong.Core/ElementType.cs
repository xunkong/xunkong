namespace Xunkong.Core;

/// <summary>
/// 元素类型
/// </summary>
[Flags]
public enum ElementType
{

    /// <summary>
    /// 未知
    /// </summary>
    None = 0,

    /// <summary>
    /// 物理
    /// </summary>
    Physics = 1,

    /// <summary>
    /// 火
    /// </summary>
    Pyro = 2,
    Fire = 2,

    /// <summary>
    /// 水
    /// </summary>
    Hydro = 4,
    Water = 4,

    /// <summary>
    /// 风
    /// </summary>
    Anemo = 8,
    Wind = 8,

    /// <summary>
    /// 雷
    /// </summary>
    Electro = 16,

    /// <summary>
    /// 草
    /// </summary>
    Dendro = 32,
    Grass = 32,

    /// <summary>
    /// 冰
    /// </summary>
    Cryo = 64,
    Ice = 64,

    /// <summary>
    /// 岩
    /// </summary>
    Geo = 128,
    Rock = 128,

}
