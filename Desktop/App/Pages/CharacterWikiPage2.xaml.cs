using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Xunkong.Desktop.Controls;
using Xunkong.SnapMetadata;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class CharacterWikiPage2 : Page
{


    public CharacterWikiPage2()
    {
        this.InitializeComponent();
        Loaded += CharacterWikiPage_Loaded;
        Unloaded += CharacterWikiPage_Unloaded;
    }



    private async void CharacterWikiPage_Loaded(object sender, RoutedEventArgs e)
    {
        await Task.Delay(60);
        InitializePage();
    }


    private void CharacterWikiPage_Unloaded(object sender, RoutedEventArgs e)
    {

    }



    [ObservableProperty]
    private List<PM_CharacterWiki_CharacterInfo2> characterInfos;


    [ObservableProperty]
    private PM_CharacterWiki_CharacterInfo2? selectedCharacter;


    [ObservableProperty]
    private List<PM_CharacterWiki_CharacterInfo2> filterCharacters;


    private void InitializePage()
    {
        try
        {
            var list = XunkongApiService.GetGenshinData<SnapAvatarInfo>();
            var maxTime = DateTime.MaxValue;
            if (AppSetting.GetValue<bool>(SettingKeys.HideUnusableCharacter))
            {
                maxTime = DateTime.Now;
            }

            CharacterInfos = list.Where(x => x.BeginTime > new DateTime(2020, 12, 15) && x.BeginTime <= maxTime)
                                 .OrderByDescending(x => x.BeginTime)
                                 .ThenByDescending(x => x.Sort)
                                 .Select(x => new PM_CharacterWiki_CharacterInfo2
                                 {
                                     CharacterInfo = x,
                                 })
                                 .Concat(list.Where(x => x.BeginTime < new DateTime(2020, 12, 15))
                                             .OrderByDescending(x => x.BeginTime)
                                             .ThenBy(x => x.Sort)
                                             .Select(x => new PM_CharacterWiki_CharacterInfo2
                                             {
                                                 CharacterInfo = x,
                                             }))
                                 .ToList();

            var promotes = XunkongApiService.GetGenshinData<SnapAvatarPromote>();
            var materials = XunkongApiService.GetGenshinData<SnapMaterial>();
            foreach (var info in CharacterInfos)
            {
                var pid = info.CharacterInfo.PromoteId;
                info.AvatarPromotes = promotes.Where(x => x.PromoteId == pid).OrderBy(x => x.Level).ToList();
                info.PromoteItems = info.CharacterInfo.CultivationItems.SelectMany(x => materials.Where(y => y.Id == x)).ToList();
                info.PromotePropString = ((FightProperty)(info.AvatarPromotes.FirstOrDefault()?.AddProperties?.LastOrDefault()?.Type ?? 0)).ToDescription();
            }

            SelectedCharacter = CharacterInfos.FirstOrDefault();
            FilterCharacters = CharacterInfos;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "初始化角色图鉴页面");
            NotificationProvider.Error(ex, "初始化角色图鉴页面");
        }
    }



    partial void OnSelectedCharacterChanged(PM_CharacterWiki_CharacterInfo2? value)
    {
        try
        {
            value?.Initialize();
            c_GridView_Filter.SelectedItem = null;
            c_GridView_Filter.SelectedItem = value;
        }
        catch (Exception ex)
        {

        }
    }



    private void c_GridView_Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.FirstOrDefault() is PM_CharacterWiki_CharacterInfo2 info)
        {
            SelectedCharacter = info;
        }
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
                FilterCharacters = CharacterInfos;
            }
            else
            {
                var queries = args.QueryText.Split(" ");
                var result = CharacterInfos.AsQueryable();
                foreach (var query in queries)
                {
                    if (int.TryParse(query, out var rarity))
                    {
                        result = result.Where(x => x.CharacterInfo.Quality == rarity);
                        continue;
                    }
                    if (query.ToLower() is "火" or "fire" or "pyro")
                    {
                        result = result.Where(x => x.CharacterInfo.FetterInfo.VisionBefore == "火");
                        continue;
                    }
                    if (query.ToLower() is "水" or "water" or "hydro")
                    {
                        result = result.Where(x => x.CharacterInfo.FetterInfo.VisionBefore == "水");
                        continue;
                    }
                    if (query.ToLower() is "风" or "wind" or "anemo")
                    {
                        result = result.Where(x => x.CharacterInfo.FetterInfo.VisionBefore == "风");
                        continue;
                    }
                    if (query.ToLower() is "雷" or "electro")
                    {
                        result = result.Where(x => x.CharacterInfo.FetterInfo.VisionBefore == "雷");
                        continue;
                    }
                    if (query.ToLower() is "草" or "grass" or "dendro")
                    {
                        result = result.Where(x => x.CharacterInfo.FetterInfo.VisionBefore == "草");
                        continue;
                    }
                    if (query.ToLower() is "冰" or "ice" or "cryo")
                    {
                        result = result.Where(x => x.CharacterInfo.FetterInfo.VisionBefore == "冰");
                        continue;
                    }
                    if (query.ToLower() is "岩" or "rock" or "geo")
                    {
                        result = result.Where(x => x.CharacterInfo.FetterInfo.VisionBefore == "岩");
                        continue;
                    }
                    var word = query;
                    if (query.Length == 2 && query[1] == '伤')
                    {
                        word = $"{query[0]}元素伤害加成";
                    }
                    if (query is "爆伤")
                    {
                        word = "暴击伤害";
                    }
                    if (query is "物伤")
                    {
                        word = "物理伤害加成";
                    }
                    result = result.Where(x => x.Birthday.Contains(word)
                                               || x.PromotePropString.Contains(word)
                                               || x.CharacterInfo.Name.Contains(word)
                                               || x.CharacterInfo.FetterInfo.Title.Contains(word)
                                               || x.CharacterInfo.FetterInfo.Native.Contains(word)
                                               || x.CharacterInfo.FetterInfo.ConstellationAfter.Contains(word)
                                               || WeaponToString(x.CharacterInfo.Weapon).Contains(word));
                }
                FilterCharacters = result.ToList();
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


    /// <summary>
    /// 预览大图
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void c_Image_GachaSplash_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(SelectedCharacter?.ShowGachaSplash))
        {
            var images = new List<WallpaperInfoEx>();
            foreach (var info in CharacterInfos)
            {
                images.Add(new WallpaperInfoEx { Url = info.GachaSplash });
                if (info.CharacterInfo.Costumes?.Count > 1)
                {
                    images.AddRange(info.CharacterInfo.Costumes.Where(x => !string.IsNullOrWhiteSpace(x.GachaSplash)).Select(x => WallpaperInfoEx.FromUri(x.GachaSplash!)));
                }
            }
            var current = images.FirstOrDefault(x => x.Url == SelectedCharacter.ShowGachaSplash);
            if (current != null)
            {
                foreach (var item in images)
                {
                    item.Url = $"https://file.xunkong.cc/assets/img/{item.Url}.png";
                }
                MainWindow.Current.SetFullWindowContent(new ImageViewer { CurrentImage = current, ImageCollection = images, EnableLoadingRing = true });
            }
        }
    }


    /// <summary>
    /// 更换衣装
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void c_StackPanel_Outfit_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is StackPanel stackPanel)
        {
            if (stackPanel.Tag is Costume outfit && SelectedCharacter != null)
            {
                SelectedCharacter.ShowGachaSplash = outfit.GachaSplash ?? SelectedCharacter.GachaSplash;
                c_Flyout_Outfit.Hide();
            }
        }
    }



}
