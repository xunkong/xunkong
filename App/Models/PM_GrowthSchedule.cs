using Xunkong.GenshinData;
using Xunkong.GenshinData.Character;
using Xunkong.GenshinData.Material;
using Xunkong.GenshinData.Weapon;
using Xunkong.Hoyolab.Activity;
using Xunkong.Hoyolab.Avatar;

namespace Xunkong.Desktop.Models;

public class PM_GrowthSchedule_TalentMaterialGroup : List<CalendarInfo>
{

    public ContentInfo Material { get; set; }

    public List<string> Days { get; set; }


    public PM_GrowthSchedule_TalentMaterialGroup(IEnumerable<CalendarInfo> collection) : base(collection)
    {

    }


}



public class PM_GrowthSchedule_BossMaterialGroup : List<CharacterInfo>
{
    public MaterialItem Material { get; set; }

    public PM_GrowthSchedule_BossMaterialGroup(IEnumerable<CharacterInfo> collection) : base(collection)
    {

    }
}



public class PM_GrowthSchedule_SelectCharacterOrWeapon
{
    public int Id { get; set; }

    public string Name { get; set; }

    public int Rarity { get; set; }

    public ElementType Element { get; set; }

    public string Icon { get; set; }

    public int Level { get; set; }

    public AvatarWeapon? AvatarWeapon { get; set; }

}



[INotifyPropertyChanged]
public partial class PM_GrowthSchedule_CharacterComputer
{

    private bool initialized;

    [ObservableProperty]
    private CharacterInfo characterInfo;

    [ObservableProperty]
    private int currentLevel;

    [ObservableProperty]
    private int startLevel = 1;

    [ObservableProperty]
    private int targetLevel = 90;

    [ObservableProperty]
    private List<PromotionCostItem>? levelCostItems;

    [ObservableProperty]
    private List<PM_GrowthSchedule_CharacterComputer_Talent> talents;

    [ObservableProperty]
    private List<PromotionCostItem>? talentCostItems;


    public void Initialize()
    {
        if (!initialized)
        {
            try
            {
                ComputeLevelConstItems();
                ComputeTalentCostItems();
                initialized = true;
            }
            catch (Exception ex)
            {

            }
        }
    }





    partial void OnStartLevelChanged(int value)
    {
        ComputeLevelConstItems();
    }


    partial void OnTargetLevelChanged(int value)
    {
        ComputeLevelConstItems();
    }



    public void ComputeLevelConstItems()
    {
        try
        {
            var currentPromoteLevel = PromotionCostItem.GetPromoteLevel(StartLevel, false);
            var targetPromoteLevel = PromotionCostItem.GetPromoteLevel(TargetLevel, false);
            var exps = CharacterLevelList.CharacterExp.Skip(StartLevel - 1).Take(TargetLevel - StartLevel).Sum();
            var expBooks = (int)Math.Ceiling((double)exps / 20000) + targetPromoteLevel - currentPromoteLevel;
            var costMora = expBooks * 4000;
            List<PromotionCostItem> costItems = new();
            if (targetPromoteLevel - currentPromoteLevel > 0)
            {
                var promotes = CharacterInfo.Promotions?.Skip(currentPromoteLevel).Take(targetPromoteLevel - currentPromoteLevel).ToList();
                if (promotes?.Any() ?? false)
                {
                    costMora += promotes.Sum(x => x.ScoinCost);
                    costItems.AddRange(promotes.SelectMany(x => x.CostItems).GroupBy(x => x.Id)
                                               .Select(x => new PromotionCostItem { Id = x.Key, Item = x.FirstOrDefault()?.Item!, Count = x.Sum(x => x.Count) })
                                               .OrderBy(x => x.Id));
                }
            }
            if (expBooks > 0)
            {
                costItems.Insert(0, new PromotionCostItem { Id = ExpBook4.Id, Item = ExpBook4, Count = expBooks });
            }
            if (costMora > 0)
            {
                costItems.Insert(0, new PromotionCostItem { Id = Mora.Id, Item = Mora, Count = costMora });
            }
            LevelCostItems = costItems;
        }
        catch (Exception ex)
        {

        }
    }



    public void ComputeTalentCostItems()
    {
        List<PromotionCostItem> costItems = new();
        int costMora = 0;
        foreach (var talent in Talents)
        {
            if (talent.TargetLevel - talent.StartLevel > 0)
            {
                foreach (var level in talent.Talent.Levels.Skip(talent.StartLevel).Take(talent.TargetLevel))
                {
                    costMora += level.CoinCost;
                    costItems.AddRange(level.CostItems);
                }
            }
        }
        if (costMora > 0)
        {
            costItems.Add(new PromotionCostItem { Id = Mora.Id, Item = Mora, Count = costMora });
        }
        TalentCostItems = costItems.GroupBy(x => x.Id).Select(x => new PromotionCostItem { Id = x.Key, Item = x.FirstOrDefault()?.Item!, Count = x.Sum(x => x.Count) }).OrderBy(x => x.Id).ToList();
    }



    public static readonly MaterialItem Mora = new MaterialItem
    {
        Id = 202,
        Name = "摩拉",
        Description = "通行大陆的钱。世界通用的共同语言，谁人都能理解的贵金属。",
        Icon = "https://file.xunkong.cc/genshin/item/UI_ItemIcon_202.png",
        ItemType = "ITEM_VIRTUAL",
        MaterialType = "MATERIAL_ADSORBATE",
        TypeDescription = "通用货币",
        RankLevel = 3,
        StackLimit = 0,
        Rank = 10
    };


    public static readonly MaterialItem ExpBook4 = new MaterialItem
    {
        Id = 104003,
        Name = "大英雄的经验",
        Description = "角色经验素材，含有20000点经验值。\n如果向往着天空岛而在大陆开始巡礼，这些经验可称得上弥足珍贵。",
        Icon = "https://file.xunkong.cc/genshin/item/UI_ItemIcon_104003.png",
        ItemType = "ITEM_MATERIAL",
        MaterialType = "MATERIAL_EXP_FRUIT",
        TypeDescription = "角色经验素材",
        RankLevel = 4,
        StackLimit = 9999,
        Rank = 100
    };



}


[INotifyPropertyChanged]
public partial class PM_GrowthSchedule_CharacterComputer_Talent
{
    public PM_GrowthSchedule_CharacterComputer? Computer { get; set; }

    public CharacterTalent Talent { get; set; }

    [ObservableProperty]
    private int currentLevel;

    [ObservableProperty]
    private int startLevel = 1;

    [ObservableProperty]
    private int targetLevel = 10;

    partial void OnStartLevelChanged(int value)
    {
        Computer?.ComputeTalentCostItems();
    }

    partial void OnTargetLevelChanged(int value)
    {
        Computer?.ComputeTalentCostItems();
    }
}



[INotifyPropertyChanged]
public partial class PM_GrowthSchedule_WeaponComputer
{

    private bool initialized;

    [ObservableProperty]
    private WeaponInfo weaponInfo;

    [ObservableProperty]
    private int maxLevel;

    [ObservableProperty]
    private int currentLevel;

    [ObservableProperty]
    private int startLevel = 1;

    [ObservableProperty]
    private int targetLevel;

    [ObservableProperty]
    private List<PromotionCostItem>? levelCostItems;


    public void Initialize()
    {
        if (!initialized)
        {
            try
            {
                MaxLevel = WeaponInfo.Rarity > 2 ? 90 : 70;
                TargetLevel = MaxLevel;
                ComputeLevelConstItems();
                initialized = true;
            }
            catch (Exception ex)
            {

            }
        }
    }



    partial void OnStartLevelChanged(int value)
    {
        ComputeLevelConstItems();
    }


    partial void OnTargetLevelChanged(int value)
    {
        ComputeLevelConstItems();
    }



    public void ComputeLevelConstItems()
    {
        try
        {
            var currentPromoteLevel = PromotionCostItem.GetPromoteLevel(StartLevel, false);
            var targetPromoteLevel = PromotionCostItem.GetPromoteLevel(TargetLevel, false);
            var curve = WeaponInfo.Rarity switch
            {
                1 => WeaponLevelList.WeaponExpRarity1,
                2 => WeaponLevelList.WeaponExpRarity2,
                3 => WeaponLevelList.WeaponExpRarity3,
                4 => WeaponLevelList.WeaponExpRarity4,
                5 => WeaponLevelList.WeaponExpRarity5,
                _ => WeaponLevelList.WeaponExpRarity5,
            };
            var exps = curve.Skip(StartLevel - 1).Take(TargetLevel - StartLevel).Sum();
            var expStones = (int)Math.Ceiling((double)exps / 10000) + targetPromoteLevel - currentPromoteLevel;
            var costMora = expStones * 1000;
            List<PromotionCostItem> costItems = new();
            if (targetPromoteLevel - currentPromoteLevel > 0)
            {
                var promotes = WeaponInfo.Promotions?.Skip(currentPromoteLevel).Take(targetPromoteLevel - currentPromoteLevel).ToList();
                if (promotes?.Any() ?? false)
                {
                    costMora += promotes.Sum(x => x.CoinCost);
                    costItems.AddRange(promotes.SelectMany(x => x.CostItems).GroupBy(x => x.Id)
                                               .Select(x => new PromotionCostItem { Id = x.Key, Item = x.FirstOrDefault()?.Item!, Count = x.Sum(x => x.Count) })
                                               .OrderBy(x => x.Id));
                }
            }
            if (expStones > 0)
            {
                costItems.Insert(0, new PromotionCostItem { Id = ExpStone3.Id, Item = ExpStone3, Count = expStones });
            }
            if (costMora > 0)
            {
                costItems.Insert(0, new PromotionCostItem { Id = Mora.Id, Item = Mora, Count = costMora });
            }
            LevelCostItems = costItems;
        }
        catch (Exception ex)
        {

        }
    }




    public static readonly MaterialItem Mora = new MaterialItem
    {
        Id = 202,
        Name = "摩拉",
        Description = "通行大陆的钱。世界通用的共同语言，谁人都能理解的贵金属。",
        Icon = "https://file.xunkong.cc/genshin/item/UI_ItemIcon_202.png",
        ItemType = "ITEM_VIRTUAL",
        MaterialType = "MATERIAL_ADSORBATE",
        TypeDescription = "通用货币",
        RankLevel = 3,
        StackLimit = 0,
        Rank = 10
    };


    public static readonly MaterialItem ExpStone3 = new MaterialItem
    {
        Id = 104013,
        Name = "精锻用魔矿",
        Description = "武器经验素材，含有10000点经验值。\n传说，精炼的矿锭中凝结了大地上战斗的记忆。而受益于这些记忆的兵器，自然也是有灵魂的了。",
        Icon = "https://file.xunkong.cc/genshin/item/UI_ItemIcon_104013.png",
        ItemType = "ITEM_MATERIAL",
        MaterialType = "MATERIAL_WEAPON_EXP_STONE",
        TypeDescription = "武器强化素材",
        RankLevel = 4,
        StackLimit = 9999,
        Rank = 5
    };



}