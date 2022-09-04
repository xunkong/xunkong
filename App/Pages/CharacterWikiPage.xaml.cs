using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using NAudio.Wave;
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


    private readonly WaveOutEvent _audioDevice;


    public CharacterWikiPage()
    {
        this.InitializeComponent();
        _audioDevice = new WaveOutEvent();
        _audioDevice.PlaybackStopped += _audioDevice_PlaybackStopped;
        Loaded += CharacterWikiPage_Loaded;
        Unloaded += CharacterWikiPage_Unloaded;
    }



    private async void CharacterWikiPage_Loaded(object sender, RoutedEventArgs e)
    {
        await Task.Delay(30);
        InitializePage();
    }


    private void CharacterWikiPage_Unloaded(object sender, RoutedEventArgs e)
    {
        _audioDevice.Dispose();
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
            CharacterInfos = liteDb.GetCollection<CharacterInfo>()
                                   .FindAll()
                                   .OrderByDescending(x => x.BeginTime)
                                   .ThenBy(x => x.SortId)
                                   .Select(x => new PM_CharacterWiki_CharacterInfo
                                   {
                                       CharacterInfo = x,
                                       PromotePropString = PropertyType.GetDescription(x.Promotions?.LastOrDefault()?.AddProps?.LastOrDefault()?.PropType)
                                   })
                                   .ToList();
            SelectedCharacter = CharacterInfos.FirstOrDefault();
            FilterCharacters = CharacterInfos;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "初始化角色图鉴页面");
            NotificationProvider.Error(ex, "初始化角色图鉴页面");
        }
    }



    partial void OnSelectedCharacterChanged(PM_CharacterWiki_CharacterInfo? value)
    {
        _audioDevice.Stop();
        value?.Initialize();
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
                                               || x.CharacterInfo.Title.Contains(word)
                                               || x.CharacterInfo.Affiliation.Contains(word)
                                               || x.CharacterInfo.ConstllationName.Contains(word)
                                               || x.CharacterInfo.WeaponType.ToDescription().Contains(word));
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



    private int randomId;

    private Button? lastPlayedButton;


    /// <summary>
    /// 播放语音
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Button_PlayVoice_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is Button button)
            {
                if (button.Tag is string { Length: > 0 } voice)
                {
                    if (button == lastPlayedButton)
                    {
                        if (_audioDevice.PlaybackState == PlaybackState.Playing)
                        {
                            _audioDevice.Stop();
                            return;
                        }
                        else
                        {
                            button.Content = new ProgressRing { Width = 16, Height = 16, Foreground = Application.Current.Resources["TextFillColorSecondaryBrush"] as Brush };
                            var id = Random.Shared.Next();
                            randomId = id;
                            var file = await FileCache.Instance.GetFromCacheAsync(new Uri(voice));
                            if (file is null || randomId != id)
                            {
                                button.Content = "\uE102";
                                return;
                            }
                            using var fr = new AudioFileReader(file.Path);
                            _audioDevice.Init(fr);
                            _audioDevice.Play();
                            button.Content = "\uE103";
                        }
                    }
                    else
                    {
                        if (lastPlayedButton != null)
                        {
                            lastPlayedButton.Content = "\uE102";
                        }
                        _audioDevice.PlaybackStopped -= _audioDevice_PlaybackStopped;
                        _audioDevice.Stop();
                        button.Content = new ProgressRing { Width = 16, Height = 16, Foreground = Application.Current.Resources["TextFillColorSecondaryBrush"] as Brush };
                        var id = Random.Shared.Next();
                        randomId = id;
                        var file = await FileCache.Instance.GetFromCacheAsync(new Uri(voice));
                        if (file is null || randomId != id)
                        {
                            button.Content = "\uE102";
                            _audioDevice.PlaybackStopped += _audioDevice_PlaybackStopped;
                            return;
                        }
                        using var fr = new AudioFileReader(file.Path);
                        _audioDevice.Init(fr);
                        _audioDevice.Play();
                        button.Content = "\uE103";
                        lastPlayedButton = button;
                        _audioDevice.PlaybackStopped += _audioDevice_PlaybackStopped;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "加载语音");
        }
    }


    /// <summary>
    /// 播放结束
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void _audioDevice_PlaybackStopped(object? sender, StoppedEventArgs e)
    {
        if (lastPlayedButton != null)
        {
            lastPlayedButton.Content = "\uE102";
        }
    }


}
