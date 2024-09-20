using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Xunkong.SnapMetadata;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class WeaponWikiPage2 : Page
{



    public WeaponWikiPage2()
    {
        this.InitializeComponent();
        Loaded += WeaponWikiPage_Loaded;
    }



    private async void WeaponWikiPage_Loaded(object sender, RoutedEventArgs e)
    {
        await Task.Delay(60);
        InitiliazePage();
    }


    [ObservableProperty]
    private List<PM_WeaponWiki_WeaponInfo2> weaponInfos;


    [ObservableProperty]
    private PM_WeaponWiki_WeaponInfo2? selectedWeapon;


    [ObservableProperty]
    private List<PM_WeaponWiki_WeaponInfo2> filterWeapons;



    private void InitiliazePage()
    {
        try
        {
            WeaponInfos = XunkongApiService.GetGenshinData<SnapWeaponInfo>()
                                           .OrderByDescending(x => x.RankLevel)
                                           .OrderByDescending(x => x.Sort)
                                           .Select(x => new PM_WeaponWiki_WeaponInfo2
                                           {
                                               WeaponInfo = x,
                                           })
                                           .ToList();
            var promotes = XunkongApiService.GetGenshinData<SnapPromote>();
            var materials = XunkongApiService.GetGenshinData<SnapMaterial>();
            foreach (var info in WeaponInfos)
            {
                var pid = info.WeaponInfo.PromoteId;
                info.WeaponPromotes = promotes.Where(x => x.PromoteId == pid).OrderBy(x => x.Level).ToList();
                info.PromoteItems = info.WeaponInfo.CultivationItems?.SelectMany(x => materials.Where(y => y.Id == x)).ToList();
                info.PromotePropString = ((FightProperty)(info.WeaponInfo.GrowCurves.LastOrDefault()?.Type ?? 0)).ToDescription();
            }
            SelectedWeapon = WeaponInfos.FirstOrDefault();
            FilterWeapons = WeaponInfos;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "初始化武器图鉴页面");
            NotificationProvider.Error(ex, "初始化武器图鉴页面");
        }
    }



    partial void OnSelectedWeaponChanged(PM_WeaponWiki_WeaponInfo2? value)
    {
        value?.Initialize();
    }


    /// <summary>
    /// 搜索
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void c_AutoSuggestBox_Search_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(args.QueryText))
            {
                FilterWeapons = WeaponInfos;
            }
            else
            {
                var queries = args.QueryText.Split(" ");
                var result = WeaponInfos.AsQueryable();
                foreach (var query in queries)
                {
                    if (int.TryParse(query, out var rarity))
                    {
                        result = result.Where(x => x.WeaponInfo.RankLevel == rarity);
                        continue;
                    }
                    result = result.Where(x => x.WeaponInfo.Name.Contains(query) || WeaponToString(x.WeaponInfo.WeaponType).Contains(query) || x.LevelProperty.SecondaryPropString.Contains(query));
                }
                FilterWeapons = result.ToList();
            }
        }
        catch (Exception ex)
        {

        }
    }


    public string WeaponToString(int value)
    {
        return value switch
        {
            1 => "单手剑",
            10 => "法器",
            11 => "双手剑",
            12 => "弓箭",
            13 => "长柄武器",
            _ => "",
        };
    }


}
