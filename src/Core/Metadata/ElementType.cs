namespace Xunkong.Core.Metadata
{

    /// <summary>
    /// 元素类型
    /// <para/>
    /// 为避免以后出现双元素甚至多元素的情况，使用 <see cref="FlagsAttribute"/>
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
        Phys = 1,

        /// <summary>
        /// 火
        /// </summary>
        Pyro = 2,

        /// <summary>
        /// 水
        /// </summary>
        Hydro = 4,

        /// <summary>
        /// 风
        /// </summary>
        Anemo = 8,

        /// <summary>
        /// 雷
        /// </summary>
        Electro = 16,

        /// <summary>
        /// 草
        /// </summary>
        Dendro = 32,

        /// <summary>
        /// 冰
        /// </summary>
        Cryo = 64,

        /// <summary>
        /// 岩
        /// </summary>
        Geo = 128,


    }
}
