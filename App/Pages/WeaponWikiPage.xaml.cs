using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Xunkong.GenshinData.Weapon;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class WeaponWikiPage : Page
{



    public WeaponWikiPage()
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
    private List<PM_WeaponWiki_WeaponInfo> weaponInfos;


    [ObservableProperty]
    private PM_WeaponWiki_WeaponInfo? selectedWeapon;


    [ObservableProperty]
    private List<PM_WeaponWiki_WeaponInfo> filterWeapons;



    private void InitiliazePage()
    {
        try
        {
            using var liteDb = DatabaseProvider.CreateLiteDB();
            WeaponInfos = liteDb.GetCollection<WeaponInfo>().FindAll().OrderByDescending(x => x.SortId)
                                .Select(x => new PM_WeaponWiki_WeaponInfo
                                {
                                    WeaponInfo = x,
                                    PromotePropString = x.Properties?.Count > 1 ? PropertyType.GetDescription(x.Properties.LastOrDefault()?.PropertyType) : ""
                                })
                                .ToList();
            SelectedWeapon = WeaponInfos.FirstOrDefault();
            FilterWeapons = WeaponInfos;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "初始化武器图鉴页面");
            NotificationProvider.Error(ex, "初始化武器图鉴页面");
        }
    }



    partial void OnSelectedWeaponChanged(PM_WeaponWiki_WeaponInfo? value)
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
                        result = result.Where(x => x.WeaponInfo.Rarity == rarity);
                        continue;
                    }
                    result = result.Where(x => x.WeaponInfo.Name.Contains(query) || x.WeaponInfo.WeaponType.ToDescription().Contains(query) || x.PromotePropString.Contains(query));
                }
                FilterWeapons = result.ToList();
            }
        }
        catch (Exception ex)
        {

        }
    }


}
