using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Xunkong.GenshinData.Character;
using Xunkong.GenshinData.Material;
using Xunkong.Hoyolab;

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



    public GrowthSchedulePage()
    {
        this.InitializeComponent();
        _hoyolabClient = ServiceProvider.GetService<HoyolabClient>()!;
        Loaded += GrowthSchedulePage_Loaded;
    }


    private async void GrowthSchedulePage_Loaded(object sender, RoutedEventArgs e)
    {
        await Task.Delay(30);
        await InitializePageAsync();
    }



    private async Task InitializePageAsync()
    {
        try
        {
            var calendars = await _hoyolabClient.GetCalendarInfosAsync();
            var talentGroup = calendars.Where(x => x.Kind == "2" && x.BreakType == "2")
                                       .GroupBy(x => x.ContentInfos.FirstOrDefault(x => x.Title.Contains("哲学")))
                                       .Where(x => x.Key != null)
                                       .OrderBy(x => x.Key!.ContentId)
                                       .Select(x => new PM_GrowthSchedule_TalentMaterialGroup(x.OrderBy(x => x.Sort.FirstOrDefault().Value)) { Material = x.Key!, Days = x.FirstOrDefault()!.DropDay.OrderBy(x => x).ToList() });
            TalentMaterialGroup.Source = talentGroup.ToList();
            var weaponGroup = calendars.Where(x => x.Kind == "2" && x.BreakType == "1")
                                       .GroupBy(x => x.ContentInfos.MaxBy(x => x.ContentId))
                                       .Where(x => x.Key != null)
                                       .OrderBy(x => x.Key!.ContentId)
                                       .Select(x => new PM_GrowthSchedule_TalentMaterialGroup(x.OrderBy(x => x.Sort.FirstOrDefault().Value)) { Material = x.Key!, Days = x.FirstOrDefault()!.DropDay.OrderBy(x => x).ToList() });
            WeaponMaterialGroup.Source = weaponGroup.ToList();

            using var liteDb = DatabaseProvider.CreateLiteDB();
            var col = liteDb.GetCollection<CharacterInfo>();
            var characters = col.Find(x => x.Id != 10000005 && x.Id != 10000007).ToList();
            BossMaterialGroup.Source = characters.GroupBy(x => x.Talents?.FirstOrDefault()?.Levels?.Skip(6)?.FirstOrDefault()?.CostItems?.LastOrDefault()?.Item, new MaterialItemComparer())
                                                 .Where(x => x.Key != null).OrderBy(x => x.Key!.Id)
                                                 .Select(x => new PM_GrowthSchedule_BossMaterialGroup(x.OrderByDescending(x => x.BeginTime).ThenBy(x => x.SortId)) { Material = x.Key! })
                                                 .ToList();
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
