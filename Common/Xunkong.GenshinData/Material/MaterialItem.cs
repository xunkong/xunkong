namespace Xunkong.GenshinData.Material;

/// <summary>
/// 材料物品
/// </summary>
public class MaterialItem
{

    public int Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// 物品类型 <see cref="GenshinData.ItemType"/>
    /// </summary>
    public string ItemType { get; set; }

    /// <summary>
    /// 材料类型 <see cref="Material.MaterialType"/>
    /// </summary>
    public string? MaterialType { get; set; }

    /// <summary>
    /// 材料类型说明
    /// </summary>
    public string? TypeDescription { get; set; }

    /// <summary>
    /// 星级
    /// </summary>
    public int RankLevel { get; set; }

    /// <summary>
    /// 背包中一格限制多少个
    /// </summary>
    public int StackLimit { get; set; }

    /// <summary>
    /// 在背包中的顺序
    /// </summary>
    public int Rank { get; set; }



}
