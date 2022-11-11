using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Xunkong.GenshinData.Character;
using Xunkong.GenshinData.Material;
using Xunkong.GenshinData.Weapon;
using Xunkong.Hoyolab;
using Xunkong.Hoyolab.Account;
using Xunkong.Hoyolab.Avatar;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class GrowthSchedulePage : Page
{


    private readonly HoyolabClient _hoyolabClient;


    private readonly HoyolabService _hoyolabService;


    public GrowthSchedulePage()
    {
        this.InitializeComponent();
        _hoyolabClient = ServiceProvider.GetService<HoyolabClient>()!;
        _hoyolabService = ServiceProvider.GetService<HoyolabService>()!;
        Loaded += GrowthSchedulePage_Loaded;
    }



    private List<CharacterInfo> characterInfos;

    private List<WeaponInfo> weaponInfos;

    private GenshinRoleInfo? GenshinRoleInfo;

    /// <summary>
    /// 养成计划内容
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<GrowthScheduleItem> growthScheduleItems;

    /// <summary>
    /// 选择角色
    /// </summary>
    [ObservableProperty]
    private List<PM_GrowthSchedule_SelectCharacterOrWeapon> selectCharacters;

    /// <summary>
    /// 选择武器
    /// </summary>
    [ObservableProperty]
    private List<PM_GrowthSchedule_SelectCharacterOrWeapon> selectWeapons;

    /// <summary>
    /// 角色计算
    /// </summary>
    [ObservableProperty]
    private PM_GrowthSchedule_CharacterComputer? characterComputer;

    /// <summary>
    /// 武器计算
    /// </summary>
    [ObservableProperty]
    private PM_GrowthSchedule_WeaponComputer? weaponComputer;


    private async void GrowthSchedulePage_Loaded(object sender, RoutedEventArgs e)
    {
        await Task.Delay(60);
        await InitializePageAsync();
    }



    private async Task InitializePageAsync()
    {
        try
        {
            using var liteDb = DatabaseProvider.CreateLiteDB();
            var col = liteDb.GetCollection<CharacterInfo>();
            characterInfos = col.Find(x => x.Id != 10000005 && x.Id != 10000007).ToList();
            // 周本掉落材料
            BossMaterialGroup.Source = characterInfos.GroupBy(x => x.Talents?.FirstOrDefault()?.Levels?.Skip(6)?.FirstOrDefault()?.CostItems?.LastOrDefault()?.Item, new MaterialItemComparer())
                                                     .Where(x => x.Key != null).OrderBy(x => x.Key!.Id)
                                                     .Select(x => new PM_GrowthSchedule_BossMaterialGroup(x.OrderByDescending(x => x.BeginTime).ThenBy(x => x.SortId)) { Material = x.Key! })
                                                     .ToList();


            var col2 = liteDb.GetCollection<WeaponInfo>();
            weaponInfos = col2.FindAll().ToList();

            using var docDb = DatabaseProvider.CreateDocDb();
            // 养成计划内容
            GrowthScheduleItems = new(docDb.GetCollection<GrowthScheduleItem>().FindAll().OrderBy(x => x.Order));
            GrowthScheduleItems.CollectionChanged += GrowthScheduleItems_CollectionChanged;

            var calendars = await _hoyolabClient.GetCalendarInfosAsync();
            var talentGroup = calendars.Where(x => x.Kind == "2" && x.BreakType == "2")
                                       .GroupBy(x => x.ContentInfos.FirstOrDefault(x => x.Title.Contains("哲学")))
                                       .Where(x => x.Key != null)
                                       .OrderBy(x => x.Key!.ContentId)
                                       .Select(x => new PM_GrowthSchedule_TalentMaterialGroup(x.OrderBy(x => x.Sort.FirstOrDefault().Value)) { Material = x.Key!, Days = x.FirstOrDefault()!.DropDay.OrderBy(x => x).ToList() });
            // 天赋材料
            TalentMaterialGroup.Source = talentGroup.ToList();
            var weaponGroup = calendars.Where(x => x.Kind == "2" && x.BreakType == "1")
                                       .GroupBy(x => x.ContentInfos.MaxBy(x => x.ContentId))
                                       .Where(x => x.Key != null)
                                       .OrderBy(x => x.Key!.ContentId)
                                       .Select(x => new PM_GrowthSchedule_TalentMaterialGroup(x.OrderBy(x => x.Sort.FirstOrDefault().Value)) { Material = x.Key!, Days = x.FirstOrDefault()!.DropDay.OrderBy(x => x).ToList() });
            // 武器材料
            WeaponMaterialGroup.Source = weaponGroup.ToList();
        }
        catch (HttpRequestException ex)
        {
            Logger.Error(ex, "初始化养成页面");
            NotificationProvider.Error("出错了", "网络不好");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "初始化养成页面");
            NotificationProvider.Error(ex, "初始化养成页面");
        }
    }







    /// <summary>
    /// 打开养成计划编辑面板
    /// </summary>
    [RelayCommand]
    private void OpenGrowthScheduleEditGrid()
    {
        c_Grid_Edit.Visibility = Visibility.Visible;
    }


    /// <summary>
    /// 关闭养成计划编辑面板
    /// </summary>
    [RelayCommand]
    private void CloseGrowthScheduleEditGrid()
    {
        c_Grid_Edit.Visibility = Visibility.Collapsed;
    }




    /// <summary>
    /// 打开角色选择面板
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task OpenCharacterSelectionFlyoutAsync()
    {
        try
        {
            if (SelectCharacters is null)
            {
                var characters = characterInfos.OrderByDescending(x => x.BeginTime).ThenBy(x => x.SortId).Select(x => new PM_GrowthSchedule_SelectCharacterOrWeapon
                {
                    Id = x.Id,
                    Name = x.Name,
                    Rarity = x.Rarity,
                    Icon = x.FaceIcon,
                    Element = x.Element,
                }).ToList();
                try
                {
                    GenshinRoleInfo = _hoyolabService.GetLastSelectedOrFirstGenshinRoleInfo();
                    if (GenshinRoleInfo != null)
                    {
                        var avatars = await _hoyolabService.GetAvatarDetailsAsync(GenshinRoleInfo);
                        foreach (var avatar in avatars)
                        {
                            if (characters.FirstOrDefault(x => x.Id == avatar.Id) is PM_GrowthSchedule_SelectCharacterOrWeapon info)
                            {
                                info.Level = avatar.Level;
                                info.AvatarWeapon = avatar.Weapon;
                            }
                        }
                        characters = characters.OrderByDescending(x => x.Level).ToList();
                    }
                }
                catch (Exception ex) when (ex is HoyolabException or HttpRequestException)
                {
                    Logger.Error(ex, "获取角色信息时，网络或权限错误");
                }
                SelectCharacters = characters;
            }
            FlyoutBase.ShowAttachedFlyout(c_Border_SelectCharacter);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "打开角色选择面板");
            NotificationProvider.Error(ex, "打开角色选择面板");
        }
    }



    /// <summary>
    /// 选择角色
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void c_GridView_SelectCharacter_ItemClick(object sender, ItemClickEventArgs e)
    {
        try
        {
            if (e.ClickedItem is PM_GrowthSchedule_SelectCharacterOrWeapon character)
            {
                if (characterInfos.FirstOrDefault(x => x.Id == character.Id) is CharacterInfo info)
                {
                    var computer = new PM_GrowthSchedule_CharacterComputer
                    {
                        CharacterInfo = info,
                        CurrentLevel = character.Level,
                        StartLevel = character.Level == 0 ? 1 : character.Level,
                        Talents = info.Talents?.Where(x => x.Levels?.Count > 1)?.Select(x => new PM_GrowthSchedule_CharacterComputer_Talent { Talent = x })?.ToList()!
                    };
                    if (character.Level > 0 && GenshinRoleInfo != null)
                    {
                        try
                        {
                            var skills = await _hoyolabService.GetAvatarSkillLevelAsync(GenshinRoleInfo, character.Id);
                            if (computer.Talents?.Any() ?? false)
                            {
                                foreach (var talent in computer.Talents)
                                {
                                    if (skills.FirstOrDefault(x => x.Id == talent.Talent.TalentId) is AvatarSkill skill)
                                    {
                                        talent.CurrentLevel = skill.CurrentLevel;
                                        talent.StartLevel = skill.CurrentLevel;
                                    }
                                }
                            }
                        }
                        catch (Exception ex) when (ex is HoyolabException or HttpRequestException)
                        {
                            Logger.Error(ex, "选择角色时，网络或权限错误");
                        }
                    }
                    if (computer.Talents?.Any() ?? false)
                    {
                        foreach (var talent in computer.Talents)
                        {
                            talent.Computer = computer;
                        }
                    }
                    computer.Initialize();
                    CharacterComputer = computer;
                }
                if (character.AvatarWeapon != null)
                {
                    if (weaponInfos.FirstOrDefault(x => x.Id == character.AvatarWeapon.Id) is WeaponInfo info2)
                    {
                        var computer = new PM_GrowthSchedule_WeaponComputer
                        {
                            WeaponInfo = info2,
                            CurrentLevel = character.AvatarWeapon.Level,
                            StartLevel = character.AvatarWeapon.Level,
                        };
                        computer.Initialize();
                        WeaponComputer = computer;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "选择角色");
            NotificationProvider.Error(ex, "选择角色");
        }
    }



    /// <summary>
    /// 打开武器选择面板
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private void OpenWeaponSelectionFlyout()
    {
        try
        {
            if (SelectWeapons is null)
            {

                var weapons = weaponInfos.OrderByDescending(x => x.SortId).Select(x => new PM_GrowthSchedule_SelectCharacterOrWeapon
                {
                    Id = x.Id,
                    Name = x.Name,
                    Rarity = x.Rarity,
                    Icon = x.Icon,
                }).ToList();
                SelectWeapons = weapons;
            }
            FlyoutBase.ShowAttachedFlyout(c_Border_SelectWeapon);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "打开武器选择面板");
            NotificationProvider.Error(ex, "打开武器选择面板");
        }
    }


    /// <summary>
    /// 选择武器
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void c_GridView_SelectWeapon_ItemClick(object sender, ItemClickEventArgs e)
    {
        try
        {
            if (e.ClickedItem is PM_GrowthSchedule_SelectCharacterOrWeapon weapon)
            {
                if (weaponInfos.FirstOrDefault(x => x.Id == weapon.Id) is WeaponInfo info)
                {
                    var computer = new PM_GrowthSchedule_WeaponComputer
                    {
                        WeaponInfo = info,
                    };
                    computer.Initialize();
                    WeaponComputer = computer;
                }
            }
            c_Flyout_SelectCharacter.Hide();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "选择武器");
            NotificationProvider.Error(ex, "选择武器");
        }
    }



    /// <summary>
    /// 添加到养成计划
    /// </summary>
    [RelayCommand]
    private void AddToGrowthScheduleList()
    {
        try
        {
            if (CharacterComputer != null)
            {
                var item = new GrowthScheduleItem
                {
                    ItemId = CharacterComputer.CharacterInfo.Id,
                    Name = CharacterComputer.CharacterInfo.Name,
                    Rarity = CharacterComputer.CharacterInfo.Rarity,
                    Icon = CharacterComputer.CharacterInfo.FaceIcon,
                    Type = 0,
                };
                if (CharacterComputer.LevelCostItems?.Any() ?? false)
                {
                    item.LevelCostItems = CharacterComputer.LevelCostItems.Select(x => new GrowthScheduleCostItem { Id = x.Id, Name = x.Item.Name, CostCount = x.Count, Icon = x.Item.Icon }).ToList();
                }
                if (CharacterComputer.TalentCostItems?.Any() ?? false)
                {
                    item.TalentCostItems = CharacterComputer.TalentCostItems.Select(x => new GrowthScheduleCostItem { Id = x.Id, Name = x.Item.Name, CostCount = x.Count, Icon = x.Item.Icon }).ToList();
                }
                if (item.LevelCostItems != null || item.TalentCostItems?.Any() != null)
                {

                    GrowthScheduleItems.Add(item);
                }
                CharacterComputer = null;
            }
            if (WeaponComputer != null)
            {
                var item = new GrowthScheduleItem
                {
                    ItemId = WeaponComputer.WeaponInfo.Id,
                    Name = WeaponComputer.WeaponInfo.Name,
                    Rarity = WeaponComputer.WeaponInfo.Rarity,
                    Icon = WeaponComputer.WeaponInfo.Icon,
                    Type = 1,
                };
                if (WeaponComputer.LevelCostItems?.Any() ?? false)
                {
                    item.LevelCostItems = WeaponComputer.LevelCostItems.Select(x => new GrowthScheduleCostItem { Id = x.Id, Name = x.Item.Name, CostCount = x.Count, Icon = x.Item.Icon }).ToList();
                }
                if (item.LevelCostItems != null)
                {
                    GrowthScheduleItems.Add(item);
                }
                WeaponComputer = null;
            }
            // 在这个方法中保存新添加的计划 GrowthScheduleItems_CollectionChanged
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "添加到养成计划");
            NotificationProvider.Error(ex, "添加到养成计划");
        }
    }


    /// <summary>
    /// 删除养成计划
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void c_Button_DeleteGrowthItem_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is MenuFlyoutItem menuFlyoutItem)
            {
                if (menuFlyoutItem.DataContext is GrowthScheduleItem item)
                {
                    if (GrowthScheduleItems.Contains(item))
                    {
                        GrowthScheduleItems.Remove(item);
                    }
                    using var docDb = DatabaseProvider.CreateDocDb();
                    var col = docDb.GetCollection<GrowthScheduleItem>();
                    col.Delete(item.Id);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "删除养成计划");
            NotificationProvider.Error(ex, "删除养成计划");
        }
    }


    private int reOrderRandomId;

    /// <summary>
    /// 养成计划重新排序，并保存
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="NotImplementedException"></exception>
    private async void GrowthScheduleItems_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        Debug.WriteLine(e.Action);
        try
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                for (int i = 0; i < GrowthScheduleItems.Count; i++)
                {
                    GrowthScheduleItems[i].Order = i;
                }
                var id = Random.Shared.Next();
                reOrderRandomId = id;
                await Task.Delay(1500);
                if (reOrderRandomId == id)
                {
                    using var docDb = DatabaseProvider.CreateDocDb();
                    var col = docDb.GetCollection<GrowthScheduleItem>();
                    col.Upsert(GrowthScheduleItems);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "养成计划重新排序，并保存");
        }
    }

    /// <summary>
    /// 单个材料完成
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void c_Button_CostItem_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is Button button)
            {
                if (button.Content is TextBlock textBlock)
                {
                    if (textBlock.DataContext is GrowthScheduleCostItem costItem)
                    {
                        costItem.IsFinish = !costItem.IsFinish;
                    }
                    if (VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(button))) is StackPanel stackPanel)
                    {
                        if (stackPanel.DataContext is GrowthScheduleItem item)
                        {
                            using var docDb = DatabaseProvider.CreateDocDb();
                            var col = docDb.GetCollection<GrowthScheduleItem>();
                            col.Upsert(item);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "单个材料完成");
        }
    }



    /// <summary>
    /// 素材比较
    /// </summary>
    private class MaterialItemComparer : IEqualityComparer<MaterialItem?>
    {
        public bool Equals(MaterialItem? x, MaterialItem? y)
        {
            return x?.Id == y?.Id;
        }

        public int GetHashCode([DisallowNull] MaterialItem? obj)
        {
            return obj.Id.GetHashCode();
        }
    }


}
