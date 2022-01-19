namespace Xunkong.Core.Metadata
{

    /// <summary>
    /// 武器类型
    /// <para/>
    /// 使用 <see cref="FlagsAttribute"/> 以适应针对不同种武器的效果
    /// </summary>
    [Flags]
    public enum WeaponType
    {

        /// <summary>
        /// 未知
        /// </summary>
        None = 0,

        /// <summary>
        /// 单手剑
        /// </summary>
        Sword = 1,

        /// <summary>
        /// 双手剑
        /// </summary>
        Catalyst = 2,

        /// <summary>
        /// 长柄武器
        /// </summary>
        Claymore = 4,

        /// <summary>
        /// 法器
        /// </summary>
        Polearm = 8,

        /// <summary>
        /// 弓箭
        /// </summary>
        Bow = 16,

    }
}
