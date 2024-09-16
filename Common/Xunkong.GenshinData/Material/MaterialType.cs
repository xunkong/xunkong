namespace Xunkong.GenshinData.Material;


/// <summary>
/// 材料类型（全大写的变量名表示尚未使用，以后会更改变量名）
/// </summary>
public abstract class MaterialType
{

    /// <summary>
    /// 虚拟物品，比如经验
    /// </summary>
    public const string Virtual = "";


    public const string MATERIAL_FAKE_ABSORBATE = "MATERIAL_FAKE_ABSORBATE";


    public const string MATERIAL_ADSORBATE = "MATERIAL_ADSORBATE";

    /// <summary>
    /// 消耗品
    /// </summary>
    public const string Consume = "MATERIAL_CONSUME";

    /// <summary>
    /// 命星
    /// </summary>
    public const string Constellation = "MATERIAL_TALENT";

    /// <summary>
    /// 角色（角色也是材料）
    /// </summary>

    public const string Avatar = "MATERIAL_AVATAR";

    /// <summary>
    /// 礼包，圣遗物匣，小蛋糕
    /// </summary>
    public const string Chest = "MATERIAL_CHEST";

    /// <summary>
    /// 回血的食物
    /// </summary>
    public const string AddHpFood = "MATERIAL_NOTICE_ADD_HP";

    /// <summary>
    /// 交换物品
    /// </summary>
    public const string Exchange = "MATERIAL_EXCHANGE";

    /// <summary>
    /// 树木
    /// </summary>
    public const string Wood = "MATERIAL_WOOD";

    /// <summary>
    /// 任务物品（东西太多，难免有重复、错误的）
    /// </summary>
    public const string Quest = "MATERIAL_QUEST";

    /// <summary>
    /// 蛐蛐
    /// </summary>
    public const string Cricket = "MATERIAL_CRICKET";

    /// <summary>
    /// 小道具
    /// </summary>
    public const string Widget = "MATERIAL_WIDGET";

    /// <summary>
    /// 元素结晶？（神瞳、绯红玉髓、流明石）
    /// </summary>
    public const string ElementCrystal = "MATERIAL_ELEM_CRYSTAL";

    /// <summary>
    /// 香气四溢的食物
    /// </summary>
    public const string SpiceFood = "MATERIAL_SPICE_FOOD";

    /// <summary>
    /// 恒动械画的齿轮
    /// </summary>
    public const string ActivityGear = "MATERIAL_ACTIVITY_GEAR";

    /// <summary>
    /// 机器人兑换券
    /// </summary>
    public const string ActivityRobot = "MATERIAL_ACTIVITY_ROBOT";

    /// <summary>
    /// 恒动械画的部件
    /// </summary>
    public const string ActivityJigsaw = "MATERIAL_ACTIVITY_JIGSAW";

    /// <summary>
    /// 食物
    /// </summary>
    public const string Food = "MATERIAL_FOOD";

    /// <summary>
    /// 角色经验书
    /// </summary>
    public const string CharacterExpFruit = "MATERIAL_EXP_FRUIT";

    /// <summary>
    /// 武器强化石
    /// </summary>
    public const string WeaponExeStone = "MATERIAL_WEAPON_EXP_STONE";

    /// <summary>
    /// 角色和武器的突破材料
    /// </summary>
    public const string AvatarWeaponMaterial = "MATERIAL_AVATAR_MATERIAL";

    /// <summary>
    /// 祝圣油膏、祝圣精华
    /// </summary>
    public const string ReliquaryMaterial = "MATERIAL_RELIQUARY_MATERIAL";

    /// <summary>
    /// 脆弱树脂、须臾树脂
    /// </summary>
    public const string MATERIAL_CONSUME_BATCH_USE = "MATERIAL_CONSUME_BATCH_USE";

    /// <summary>
    /// 鱼饵
    /// </summary>
    public const string FishBait = "MATERIAL_FISH_BAIT";

    /// <summary>
    /// 礼包
    /// </summary>
    public const string MATERIAL_CHEST_BATCH_USE = "MATERIAL_CHEST_BATCH_USE";

    /// <summary>
    /// 可选礼包
    /// </summary>
    public const string SelectableChest = "MATERIAL_SELECTABLE_CHEST";

    /// <summary>
    /// 种子
    /// </summary>
    public const string Seed = "MATERIAL_HOME_SEED";

    /// <summary>
    /// 风之翼
    /// </summary>
    public const string Flycloak = "MATERIAL_FLYCLOAK";

    /// <summary>
    /// 旋曜玉帛 BGM
    /// </summary>
    public const string MATERIAL_BGM = "MATERIAL_BGM";

    /// <summary>
    /// 霄灯
    /// </summary>
    public const string SeaLamp = "MATERIAL_SEA_LAMP";

    /// <summary>
    /// 导能圆盘的Buff
    /// </summary>
    public const string ChannellerSlabBuff = "MATERIAL_CHANNELLER_SLAB_BUFF";

    /// <summary>
    /// 鱼竿
    /// </summary>
    public const string FishRod = "MATERIAL_FISH_ROD";

    /// <summary>
    /// 名片
    /// </summary>
    public const string NameCard = "MATERIAL_NAMECARD";

    /// <summary>
    /// 烟花
    /// </summary>
    public const string Fireworks = "MATERIAL_FIREWORKS";

    /// <summary>
    /// 服装
    /// </summary>
    public const string Costume = "MATERIAL_COSTUME";

    /// <summary>
    /// 摆设套件图纸
    /// </summary>
    public const string FurnitureSuiteFormula = "MATERIAL_FURNITURE_SUITE_FORMULA";

    /// <summary>
    /// 摆设套件图纸
    /// </summary>
    public const string FurnitureFormula = "MATERIAL_FURNITURE_FORMULA";


}
