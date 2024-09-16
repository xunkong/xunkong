namespace Xunkong.GenshinData.Material;

/// <summary>
/// 不同种类材料物品的所有属性，仅用于说明
/// </summary>
public class MaterialBase
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Icon { get; set; }

    /// <summary>
    /// 星级
    /// </summary>
    public int RankLevel { get; set; }

    /// <summary>
    /// 物品分类 <see cref="GenshinData.ItemType"/>
    /// </summary>
    public string? ItemType { get; set; }

    /// <summary>
    /// 材料分类 <see cref="Material.MaterialType"/>
    /// </summary>
    public string? MaterialType { get; set; }

    /// <summary>
    /// 背包中一格限制多少个
    /// </summary>
    public int StackLimit { get; set; }

    /// <summary>
    /// 效果描述
    /// </summary>
    public string? EffectDescription { get; set; }

    /// <summary>
    /// 特殊描述
    /// </summary>
    public string? SpecialDescription { get; set; }

    /// <summary>
    /// 类型描述
    /// </summary>
    public string? TypeDescription { get; set; }

    /// <summary>
    /// 名片的图片
    /// </summary>
    public List<string> PicPath { get; set; }


    /// <summary>
    /// 名片好友列表背景图
    /// </summary>
    public string? GalleryBackground { get; set; }

    /// <summary>
    /// 名片个人展示图
    /// </summary>
    public string? ProfileImage { get; set; }

    /// <summary>
    /// buff 图标
    /// </summary>
    public string? EffectIcon { get; set; }


    /// <summary>
    /// 在背包中的顺序
    /// </summary>
    public int Rank { get; set; }

    /// <summary>
    /// 书籍集合
    /// </summary>
    public int SetID { get; set; }

    /// <summary>
    /// 食物品质
    /// </summary>
    public string? FoodQuality { get; set; }

    /// <summary>
    /// 食物冷却
    /// </summary>
    public int CdTime { get; set; }


    public long NameTextMapHash { get; set; }
    public long DescTextMapHash { get; set; }
    public long EffectDescTextMapHash { get; set; }
    public long SpecialDescTextMapHash { get; set; }
    public long TypeDescTextMapHash { get; set; }




}