using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Xunkong.Desktop.Controls;
using Xunkong.GenshinData.Character;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class CharacterWikiPage : Page
{


    public CharacterWikiPage()
    {
        this.InitializeComponent();
        Loaded += CharacterWikiPage_Loaded;
    }

    private async void CharacterWikiPage_Loaded(object sender, RoutedEventArgs e)
    {
        await Task.Delay(60);
        InitializePage();
    }



    [ObservableProperty]
    private List<PM_CharacterWiki_CharacterInfo> characterInfos;


    [ObservableProperty]
    private PM_CharacterWiki_CharacterInfo? selectedCharacter;


    [ObservableProperty]
    private List<PM_CharacterWiki_CharacterInfo> filterCharacters;


    private void InitializePage()
    {
        try
        {
            using var liteDb = DatabaseProvider.CreateLiteDB();
            CharacterInfos = liteDb.GetCollection<CharacterInfo>().FindAll().OrderByDescending(x => x.BeginTime).ThenBy(x => x.SortId).Select(x => new PM_CharacterWiki_CharacterInfo { CharacterInfo = x }).ToList();
            SelectedCharacter = CharacterInfos.FirstOrDefault();
            FilterCharacters = CharacterInfos;
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
    }



    partial void OnSelectedCharacterChanged(PM_CharacterWiki_CharacterInfo? value)
    {
        value?.Initialize();
        c_ScrollViewer_Base.ScrollToVerticalOffset(0);
        c_GridView_Filter.SelectedItem = null;
        c_GridView_Filter.SelectedItem = value;
    }



    private void c_GridView_Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.FirstOrDefault() is PM_CharacterWiki_CharacterInfo info)
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
                        result = result.Where(x => x.CharacterInfo.Rarity == rarity);
                        continue;
                    }
                    if (query.ToLower() is "火" or "fire" or "pyro")
                    {
                        result = result.Where(x => x.CharacterInfo.Element == ElementType.Fire);
                        continue;
                    }
                    if (query.ToLower() is "水" or "water" or "hydro")
                    {
                        result = result.Where(x => x.CharacterInfo.Element == ElementType.Water);
                        continue;
                    }
                    if (query.ToLower() is "风" or "wind" or "anemo")
                    {
                        result = result.Where(x => x.CharacterInfo.Element == ElementType.Wind);
                        continue;
                    }
                    if (query.ToLower() is "雷" or "electro")
                    {
                        result = result.Where(x => x.CharacterInfo.Element == ElementType.Electro);
                        continue;
                    }
                    if (query.ToLower() is "草" or "grass" or "dendro")
                    {
                        result = result.Where(x => x.CharacterInfo.Element == ElementType.Grass);
                        continue;
                    }
                    if (query.ToLower() is "冰" or "ice" or "cryo")
                    {
                        result = result.Where(x => x.CharacterInfo.Element == ElementType.Ice);
                        continue;
                    }
                    if (query.ToLower() is "岩" or "rock" or "geo")
                    {
                        result = result.Where(x => x.CharacterInfo.Element == ElementType.Rock);
                        continue;
                    }
                    result = result.Where(x => x.CharacterInfo.Name.Contains(query)
                                               || x.CharacterInfo.Title.Contains(query)
                                               || x.CharacterInfo.ConstllationName.Contains(query)
                                               || x.CharacterInfo.WeaponType.ToDescription().Contains(query));
                }
                FilterCharacters = result.ToList();
            }
        }
        catch (Exception ex)
        {

        }
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
            var images = new List<string>();
            foreach (var info in CharacterInfos)
            {
                images.Add(info.CharacterInfo.GachaSplash);
                if (info.CharacterInfo.Outfits?.Count > 1)
                {
                    images.AddRange(info.CharacterInfo.Outfits.Select(x => x.GachaSplash!).Where(x => !string.IsNullOrWhiteSpace(x)));
                }
            }
            MainWindow.Current.SetFullWindowContent(new ImageViewer { Source = SelectedCharacter.ShowGachaSplash, SourceCollection = images });
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
            if (stackPanel.Tag is CharacterOutfit outfit && SelectedCharacter != null)
            {
                SelectedCharacter.ShowGachaSplash = outfit.GachaSplash ?? SelectedCharacter.CharacterInfo.GachaSplash;
                c_Flyout_Outfit.Hide();
            }
        }
    }


}
