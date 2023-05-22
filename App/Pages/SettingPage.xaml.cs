using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Net.Http;
using Windows.System;
using Xunkong.Desktop.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class SettingPage : Page
{

    private readonly HttpClient _httpClient;

    private readonly XunkongApiService _xunkongApiService;

    private readonly UpdateService _updateService;

    public string AppName => XunkongEnvironment.AppName;

    public string AppVersion => XunkongEnvironment.AppVersion.ToString();

    public string UserDataPath => XunkongEnvironment.UserDataPath;



    public SettingPage()
    {
        this.InitializeComponent();
        _httpClient = ServiceProvider.GetService<HttpClient>()!;
        _xunkongApiService = ServiceProvider.GetService<XunkongApiService>()!;
        _updateService = ServiceProvider.GetService<UpdateService>()!;
        Loaded += SettingPage_Loaded;
    }



    private void SettingPage_Loaded(object sender, RoutedEventArgs e)
    {
        ComputeCachedFileTotalSize();
    }



    #region Introduce and Good


    /// <summary>
    /// 点击推荐图片
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _Image_RecommendWallpaper_Tapped(object sender, TappedRoutedEventArgs e)
    {
        MainWindow.Current.SetFullWindowContent(new ImageViewer { CurrentImage = HomePage.FallbackWallpaper });
    }

    /// <summary>
    /// 超链接点击
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private async void Hyperlink_Click(Microsoft.UI.Xaml.Documents.Hyperlink sender, Microsoft.UI.Xaml.Documents.HyperlinkClickEventArgs args)
    {
        await Launcher.LaunchUriAsync(sender.NavigateUri);
    }



    [RelayCommand]
    private void CopyDevelopersMail()
    {
        ClipboardHelper.SetText("scighost@outlook.com");
        NotificationProvider.Success("已复制开发者的邮件（如果没收到回复，可能是被识别为垃圾邮件了）", 5000);
    }


    /// <summary>
    /// 显示更新内容
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void HyperlinkButton_UpdateContent_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            MainPage.Current.Navigate(typeof(UpdateContentPage));
        }
        catch { }
    }


    /// <summary>
    /// 打开商店页面
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void HyperlinkButton_OpenStore_Click(object sender, RoutedEventArgs e)
    {
        await Launcher.LaunchUriAsync(new("ms-windows-store://pdp/?productid=9N2SVG0JMT12"));
    }


    #endregion



    #region Version and Theme



    /// <summary>
    /// 检查更新
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task CheckUpdateAsync()
    {
        try
        {
            OperationHistory.AddToDatabase("CheckUpdate", XunkongEnvironment.AppVersion.ToString());
            Logger.TrackEvent("CheckUpdate", "Version", XunkongEnvironment.AppVersion.ToString(), "IsStore", XunkongEnvironment.IsStoreVersion.ToString());
            var update = await _updateService.CheckUpdateAsync(true);

            if (update.Github is GithubService.GithubRelease release)
            {
                var infoBar = NotificationProvider.Create(InfoBarSeverity.Success, release.Prerelease ? $"新预览版 {release.TagName}" : $"新版本 {release.TagName}", release.Name, "详细信息", async () => await Launcher.LaunchUriAsync(new Uri(release.HtmlUrl)));
                NotificationProvider.Show(infoBar);
            }
            if (update.Store is not null)
            {
                var infoBar = NotificationProvider.Create(InfoBarSeverity.Success, "薛定谔的更新", "只有安装后才知道是不是真的更新", "下载并安装", () => _updateService.RequestDownloadStoreNewVersion(update.Store));
                NotificationProvider.Show(infoBar);
            }
            if (update is (null, null))
            {
                NotificationProvider.Success("已是最新版本");
            }
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex);
            Logger.Error(ex, "检查更新");
        }
    }


    /// <summary>
    /// 是否可以编辑启用预览版
    /// </summary>
    public bool CanEditPrerelease => XunkongEnvironment.IsStoreVersion;


    /// <summary>
    /// 启用预览版
    /// </summary>
    [ObservableProperty]
    private bool _EnablePrerelease = !XunkongEnvironment.IsStoreVersion || AppSetting.GetValue<bool>(SettingKeys.EnablePrerelease);
    partial void OnEnablePrereleaseChanged(bool value)
    {
        AppSetting.SetValue(SettingKeys.EnablePrerelease, value);
    }



    /// <summary>
    /// 启用主页壁纸
    /// </summary>
    [ObservableProperty]
    private bool _EnableHomePageWallpaper = AppSetting.GetValue(SettingKeys.EnableHomePageWallpaper, true);
    partial void OnEnableHomePageWallpaperChanged(bool value)
    {
        AppSetting.SetValue(SettingKeys.EnableHomePageWallpaper, value);
    }



    /// <summary>
    /// 切换应用主题
    /// </summary>
    [ObservableProperty]
    private int _SelectedThemeIndex = AppSetting.GetValue<int>(SettingKeys.ApplicationTheme);
    partial void OnSelectedThemeIndexChanged(int value)
    {
        AppSetting.SetValue(SettingKeys.ApplicationTheme, value);
        var point = FontIcon_Theme.TransformToVisual(MainWindow.Current.Content).TransformPoint(new Windows.Foundation.Point(0, 0));
        var center = new System.Numerics.Vector2(((float)(point.X + FontIcon_Theme.ActualWidth / 2)), ((float)(point.Y + FontIcon_Theme.ActualHeight / 2)));
        MainWindow.Current.ChangeApplicationTheme(value, center);
        OperationHistory.AddToDatabase("ChangeTheme", "SelectItem");
        Logger.TrackEvent("ChangeTheme", "Type", "SelectItem");
    }



    /// <summary>
    /// 按浏览计费的网络中仍然下载图片
    /// </summary>
    [ObservableProperty]
    private bool _DownloadWallpaperOnMeteredInternet = AppSetting.GetValue<bool>(SettingKeys.DownloadWallpaperOnMeteredInternet);
    partial void OnDownloadWallpaperOnMeteredInternetChanged(bool value)
    {
        AppSetting.SetValue(SettingKeys.DownloadWallpaperOnMeteredInternet, value);
    }



    /// <summary>
    /// 窗口背景材质
    /// </summary>
    [ObservableProperty]
    private int _WindowBackdropIndex = (int)AppSetting.GetValue<uint>(SettingKeys.WindowBackdrop) & 0xF;
    partial void OnWindowBackdropIndexChanged(int value)
    {
        ChangeWindowBackdrop((AlwaysActiveBackdrop ? 0x80000000 : 0) | (uint)value);
    }


    /// <summary>
    /// 始终激活背景
    /// </summary>
    [ObservableProperty]
    private bool _AlwaysActiveBackdrop = (AppSetting.GetValue<uint>(SettingKeys.WindowBackdrop) & 0x80000000) > 0;
    partial void OnAlwaysActiveBackdropChanged(bool value)
    {
        ChangeWindowBackdrop((value ? 0x80000000 : 0) | (uint)WindowBackdropIndex);
    }


    /// <summary>
    /// 修改背景材质
    /// </summary>
    /// <param name="value"></param>
    private void ChangeWindowBackdrop(uint value)
    {
        if (MainWindow.Current.TryChangeBackdrop(value))
        {
            AppSetting.SetValue(SettingKeys.WindowBackdrop, value);
        }
        else
        {
            AppSetting.SetValue(SettingKeys.WindowBackdrop, 0);
            NotificationProvider.Warning("不支持此操作");
        }
    }


    /// <summary>
    /// 使用自定义壁纸
    /// </summary>
    [ObservableProperty]
    private bool _UseCustomWallpaper = AppSetting.GetValue<bool>(SettingKeys.UseCustomWallpaper);
    partial void OnUseCustomWallpaperChanged(bool value)
    {
        AppSetting.SetValue(SettingKeys.UseCustomWallpaper, value);
    }


    /// <summary>
    /// 壁纸保存位置
    /// </summary>
    [ObservableProperty]
    private string _WallpaperSaveFolder = AppSetting.GetValue<string>(SettingKeys.WallpaperSaveFolder) ?? Path.Combine(XunkongEnvironment.UserDataPath, "Wallpaper");



    /// <summary>
    /// 修改壁纸保存位置
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task ChangeWallpaperSaveFolderAsync()
    {
        try
        {
            var folder = await FileDialogHelper.PickFolderAsync(MainWindow.Current.HWND);
            if (Directory.Exists(folder))
            {
                WallpaperSaveFolder = folder;
                AppSetting.SetValue(SettingKeys.WallpaperSaveFolder, folder);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "修改壁纸保存位置");
            NotificationProvider.Error(ex, "修改壁纸保存位置");
        }
    }



    /// <summary>
    /// 壁纸图片格式
    /// </summary>
    [ObservableProperty]
    private string? _WallpaperRequestFormat = AppSetting.GetValue<string>(SettingKeys.WallpaperRequestFormat);
    partial void OnWallpaperRequestFormatChanged(string? value)
    {
        AppSetting.SetValue(SettingKeys.WallpaperRequestFormat, value);
        XunkongApiService.WallpaperRequestFormat = value;
        XunkongApiService.ChangeWallpaperFileExtension(value);
    }




    #endregion



    #region Start Game



    [ObservableProperty]
    private int _GameServerIndex = AppSetting.GetValue<int>(SettingKeys.GameServerIndex);
    partial void OnGameServerIndexChanged(int value)
    {
        AppSetting.SetValue(SettingKeys.GameServerIndex, value);
    }

    /// <summary>
    /// 游戏 exe 文件路径
    /// </summary>
    [ObservableProperty]
    private string? _GameExePathCN = AppSetting.GetValue(SettingKeys.GameExePathCN, "不指定具体文件则会从注册表查找文件路径");

    [ObservableProperty]
    private string? _GameExePathGlobal = AppSetting.GetValue(SettingKeys.GameExePathGlobal, "不指定具体文件则会从注册表查找文件路径");

    [ObservableProperty]
    private string? _GameExePathCNCloud = AppSetting.GetValue(SettingKeys.GameExePathCNCloud, "不指定具体文件则会从注册表查找文件路径");

    /// <summary>
    /// 解锁 fps 上限
    /// </summary>
    [ObservableProperty]
    private int _UnlockedFPS = AppSetting.GetValue(SettingKeys.TargetFPS, 60);
    partial void OnUnlockedFPSChanged(int value)
    {
        AppSetting.SetValue(SettingKeys.TargetFPS, value);
    }

    /// <summary>
    /// 使用无边框窗口
    /// </summary>
    [ObservableProperty]
    private bool _UsePopupWindow = AppSetting.GetValue<bool>(SettingKeys.IsPopupWindow);
    partial void OnUsePopupWindowChanged(bool value)
    {
        AppSetting.SetValue(SettingKeys.IsPopupWindow, value);
    }


    /// <summary>
    /// 游戏窗口宽度
    /// </summary>
    [ObservableProperty]
    private int _StartGameWindowWidth = AppSetting.GetValue<int>(SettingKeys.StartGameWindowWidth);
    partial void OnStartGameWindowWidthChanged(int value)
    {
        AppSetting.SetValue(SettingKeys.StartGameWindowWidth, value);
    }

    /// <summary>
    /// 游戏窗口高度
    /// </summary>
    [ObservableProperty]
    private int _StartGameWindowHeight = AppSetting.GetValue<int>(SettingKeys.StartGameWindowHeight);
    partial void OnStartGameWindowHeightChanged(int value)
    {
        AppSetting.SetValue(SettingKeys.StartGameWindowHeight, value);
    }



    /// <summary>
    /// 启动游戏
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task StartGameAsync()
    {
        try
        {
            await GameAccountService.StartGameAsync(GameServerIndex, true);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "启动游戏");
            NotificationProvider.Error(ex, "启动游戏");
        }
    }


    /// <summary>
    /// 更改游戏位置
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task ChangeGameExePathAsync(string server)
    {
        try
        {
            var file = await FileDialogHelper.PickSingleFileAsync(MainWindow.Current.HWND, new List<(string, string)> { ("Executable", "*.exe") });
            if (File.Exists(file))
            {
                if (file.EndsWith(".exe"))
                {
                    switch (server)
                    {
                        case "0":
                            AppSetting.SetValue(SettingKeys.GameExePathCN, file);
                            GameExePathCN = file;
                            break;
                        case "1":
                            AppSetting.SetValue(SettingKeys.GameExePathGlobal, file);
                            GameExePathGlobal = file;
                            break;
                        case "2":
                            AppSetting.SetValue(SettingKeys.GameExePathCNCloud, file);
                            GameExePathCNCloud = file;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    NotificationProvider.Warning("文件名不太对", 3000);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "设置游戏 exe 文件路径");
            NotificationProvider.Error(ex, "设置游戏 exe 文件路径");
        }
    }



    [ObservableProperty]
    private bool _CheckTransformerAndHomeCoinWhenStartGame = AppSetting.GetValue(SettingKeys.CheckTransformerAndHomeCoinWhenStartGame, true);
    partial void OnCheckTransformerAndHomeCoinWhenStartGameChanged(bool value)
    {
        AppSetting.SetValue(SettingKeys.CheckTransformerAndHomeCoinWhenStartGame, value);
    }




    #endregion



    #region Hoyolab Check in Task


    /// <summary>
    /// 启动应用时签到所有账号
    /// </summary>
    [ObservableProperty]
    private bool _SignInAllAccountsWhenStartUpApplication = AppSetting.GetValue<bool>(SettingKeys.SignInAllAccountsWhenStartUpApplication);
    partial void OnSignInAllAccountsWhenStartUpApplicationChanged(bool value)
    {
        AppSetting.SetValue(SettingKeys.SignInAllAccountsWhenStartUpApplication, value);
    }



    /// <summary>
    /// 后台定时签到
    /// </summary>
    [ObservableProperty]
    private bool _isRegisterCheckInScheduler = TaskSchedulerService.IsDailyCheckInEnable();


    partial void OnIsRegisterCheckInSchedulerChanged(bool value)
    {
        try
        {
            TaskSchedulerService.RegisterForDailyCheckIn(value);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "注册签到任务");
            NotificationProvider.Error(ex, "注册签到任务");
        }
    }



    #endregion



    #region Home Page


    /// <summary>
    /// 显示实时便笺
    /// </summary>
    [ObservableProperty]
    private bool _DisableDailyNotesInHomePage = AppSetting.GetValue(SettingKeys.EnableDailyNotesInHomePage, true);
    partial void OnDisableDailyNotesInHomePageChanged(bool value)
    {
        AppSetting.SetValue(SettingKeys.EnableDailyNotesInHomePage, value);
    }

    /// <summary>
    /// 实时便笺缓存时间
    /// </summary>
    [ObservableProperty]
    private int _DailyNoteCacheMinutes = AppSetting.GetValue<int>(SettingKeys.DailyNoteCacheMinutes);
    partial void OnDailyNoteCacheMinutesChanged(int value)
    {
        AppSetting.SetValue(SettingKeys.DailyNoteCacheMinutes, value);
    }


    /// <summary>
    /// 留影叙佳期
    /// </summary>
    //[ObservableProperty]
    //private bool _EnableBirthdayStarInHomePage = AppSetting.GetValue(SettingKeys.EnableBirthdayStarInHomePage, false);
    //partial void OnEnableBirthdayStarInHomePageChanged(bool value)
    //{
    //    AppSetting.SetValue(SettingKeys.EnableBirthdayStarInHomePage, value);
    //}


    /// <summary>
    /// 即将结束的活动
    /// </summary>
    [ObservableProperty]
    private bool _EnableFinishingActivityInHomePage = AppSetting.GetValue(SettingKeys.EnableFinishingActivityInHomePage, true);
    partial void OnEnableFinishingActivityInHomePageChanged(bool value)
    {
        AppSetting.SetValue(SettingKeys.EnableFinishingActivityInHomePage, value);
    }


    /// <summary>
    /// 导航到实时便笺设置页面
    /// </summary>
    [RelayCommand]
    private void NavigateToDailyNoteSettingPage()
    {
        MainPage.Current.Navigate(typeof(DailyNoteSettingPage));
    }


    #endregion



    #region WebTool


    /// <summary>
    /// 导航到网页快捷方式编辑页面
    /// </summary>
    [RelayCommand]
    private void NavigateToWebToolEditPage()
    {
        try
        {
            MainPage.Current.Navigate(typeof(WebToolEditPage));
        }
        catch { }
    }

    #endregion



    #region DailyNote Tile

    /// <summary>
    /// 自动磁贴刷新
    /// </summary>
    [ObservableProperty]
    private bool _EnableDailyNoteTask = AppSetting.GetValue(SettingKeys.EnableDailyNoteTask, true);
    partial void OnEnableDailyNoteTaskChanged(bool value)
    {
        AppSetting.SetValue(SettingKeys.EnableDailyNoteTask, value);
        try
        {
            TaskSchedulerService.UnegisterForRefreshTile();
            if (value)
            {
                TaskSchedulerService.RegisterForRefreshTile();
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
    }

    /// <summary>
    /// 磁贴自动刷新时间间隔
    /// </summary>
    [ObservableProperty]
    private int _DailyNoteTaskTimeInterval = AppSetting.GetValue(SettingKeys.DailyNoteTaskTimeInterval, 16);
    partial void OnDailyNoteTaskTimeIntervalChanged(int value)
    {
        AppSetting.SetValue(SettingKeys.DailyNoteTaskTimeInterval, value);
        try
        {
            TaskSchedulerService.UnegisterForRefreshTile();
            TaskSchedulerService.RegisterForRefreshTile();
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
    }



    #endregion



    #region Other Function


    /// <summary>
    /// 页面缓存
    /// </summary>
    [ObservableProperty]
    private bool _EnableNavigationCache = AppSetting.GetValue<bool>(SettingKeys.EnableNavigationCache);
    partial void OnEnableNavigationCacheChanged(bool value)
    {
        AppSetting.SetValue(SettingKeys.EnableNavigationCache, value);
    }


    /// <summary>
    /// 使用旧版我的角色页面
    /// </summary>
    [ObservableProperty]
    private bool _UseCharacterPageOldVersion = AppSetting.GetValue<bool>(SettingKeys.UseCharacterPageOldVersion);
    partial void OnUseCharacterPageOldVersionChanged(bool value)
    {
        AppSetting.SetValue(SettingKeys.UseCharacterPageOldVersion, value);
    }


    /// <summary>
    /// 隐藏未上线的角色
    /// </summary>
    [ObservableProperty]
    private bool _HideUnusableCharacter = AppSetting.GetValue<bool>(SettingKeys.HideUnusableCharacter);
    partial void OnHideUnusableCharacterChanged(bool value)
    {
        AppSetting.SetValue(SettingKeys.HideUnusableCharacter, value);
    }


    /// <summary>
    /// 显示新手祈愿统计
    /// </summary>
    [ObservableProperty]
    private bool _ShowNoviceWishType = AppSetting.GetValue<bool>(SettingKeys.ShowNoviceWishType);
    partial void OnShowNoviceWishTypeChanged(bool value)
    {
        AppSetting.SetValue(SettingKeys.ShowNoviceWishType, value);
    }



    #endregion



    #region Data


    /// <summary>
    /// 截图备份文件夹
    /// </summary>
    [ObservableProperty]
    private string _GameScreenshotBackupFolder = AppSetting.GetValue<string>(SettingKeys.GameScreenshotBackupFolder) ?? Path.Combine(XunkongEnvironment.UserDataPath, "Screenshot");
    partial void OnGameScreenshotBackupFolderChanged(string value)
    {
        AppSetting.SetValue(SettingKeys.GameScreenshotBackupFolder, value);
    }


    /// <summary>
    /// 修改截图备份文件夹
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task ChangeScreenshotBackupFolderAsync()
    {
        try
        {
            var folder = await FileDialogHelper.PickFolderAsync(MainWindow.Current.HWND);
            if (Directory.Exists(folder))
            {
                GameScreenshotBackupFolder = folder;
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "设置截图备份文件夹");
            NotificationProvider.Error(ex, "设置截图备份文件夹");
        }
    }




    /// <summary>
    /// 打开用户数据文件夹
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task OpenUserDataPath()
    {
        try
        {
            if (Directory.Exists(UserDataPath))
            {
                await Launcher.LaunchFolderPathAsync(UserDataPath);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "打开用户数据文件夹");
        }
    }




    /// <summary>
    /// 图片已缓存 xxx MB
    /// </summary>
    [ObservableProperty]
    private string _imageCacheSizeString;


    /// <summary>
    /// 语音缓存
    /// </summary>
    [ObservableProperty]
    private string _voiceCacheSizeString;


    /// <summary>
    /// 缩略图缓存
    /// </summary>
    [ObservableProperty]
    private string _thumbnailCacheSizeString;


    /// <summary>
    /// 全部缓存
    /// </summary>
    [ObservableProperty]
    private string _totalCacheSizeString;


    /// <summary>
    /// 计算已缓存文件的大小
    /// </summary>
    private async void ComputeCachedFileTotalSize()
    {
        try
        {
            long totalSize = 0;

            var size = GetFolderSize((await XunkongCache.Instance.GetCacheFolderAsync()).Path);
            totalSize += size;
            ImageCacheSizeString = GetSizeString(size);

            size = GetFolderSize((await VoiceCache.Instance.GetCacheFolderAsync()).Path);
            totalSize += size;
            VoiceCacheSizeString = GetSizeString(size);

            size = GetFolderSize((await ThumbnailCache.GetCacheFolderAsync()).Path);
            totalSize += size;
            ThumbnailCacheSizeString = GetSizeString(size);

            TotalCacheSizeString = GetSizeString(totalSize);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "计算已缓存文件的大小");
        }
    }



    private long GetFolderSize(string folder)
    {
        if (Directory.Exists(folder))
        {
            return new DirectoryInfo(folder).GetFiles("*", SearchOption.AllDirectories).Sum(x => x.Length);
        }
        else
        {
            return 0;
        }
    }


    private string GetSizeString(long size)
    {
        if (size > 0)
        {

            return $"已缓存 {(double)size / (1 << 20):F2} MB";
        }
        else
        {
            return $"已缓存 0 MB";
        }
    }




    /// <summary>
    /// 清除缓存
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task ClearCacheAsync(string cacheMode)
    {
        try
        {
            switch (cacheMode)
            {
                case "ClearAll":
                    await DeleteFolderAsync((await XunkongCache.Instance.GetCacheFolderAsync()).Path);
                    CachedImage.ClearCache();
                    await DeleteFolderAsync((await VoiceCache.Instance.GetCacheFolderAsync()).Path);
                    await DeleteFolderAsync((await ThumbnailCache.GetCacheFolderAsync()).Path);
                    ThumbnailCache.ClearCache();
                    break;
                case "ClearImage":
                    await DeleteFolderAsync((await XunkongCache.Instance.GetCacheFolderAsync()).Path);
                    CachedImage.ClearCache();
                    break;
                case "ClearVoice":
                    await DeleteFolderAsync((await VoiceCache.Instance.GetCacheFolderAsync()).Path);
                    break;
                case "ClearThumbnail":
                    await DeleteFolderAsync((await ThumbnailCache.GetCacheFolderAsync()).Path);
                    ThumbnailCache.ClearCache();
                    break;
                default:
                    break;
            }

            NotificationProvider.Success($"完成");
            OperationHistory.AddToDatabase("ClearCache", cacheMode);
            Logger.TrackEvent("ClearCache", "CacheMode", cacheMode);
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex, "清除缓存");
            Logger.Error(ex, "清除缓存");
        }
    }



    private async Task DeleteFolderAsync(string folder)
    {
        await Task.Run(() =>
        {
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }
            Directory.CreateDirectory(folder);
        });
    }





    /// <summary>
    /// 预缓存所有图片
    /// </summary>
    [RelayCommand]
    private void StartPreCacheAllImages()
    {
        PreCacheService.StartPreCache();
    }


    /// <summary>
    /// 手动更新原神数据
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task UpdateGenshinDataAsync()
    {
        try
        {
            await _xunkongApiService.GetAllGenshinDataFromServerAsync();
            UpdateGenshinDataText = $"数据量较大，请耐心等待。上次更新：刚刚";
            NotificationProvider.Success("完成", "本地的原神数据已是最新版本");
            OperationHistory.AddToDatabase("UpdateGenshinData");
            Logger.TrackEvent("UpdateGenshinData");
        }
        catch (HttpRequestException ex)
        {
            Logger.Error(ex, "手动更新原神数据");
            NotificationProvider.Error("出错了", "网络不好");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "手动更新原神数据");
            NotificationProvider.Error(ex, "手动更新原神数据");
        }
    }



    [ObservableProperty]
    private string _UpdateGenshinDataText = GetUpdateGenshinDataText();



    private static string GetUpdateGenshinDataText()
    {
        try
        {
            if (File.Exists(DatabaseProvider.GenshinDataDbPath))
            {
                var time = DateTime.Now - new FileInfo(DatabaseProvider.GenshinDataDbPath).LastWriteTime;
                if (time.Days > 0)
                {
                    return $"数据量较大，请耐心等待。上次更新：{time.Days}天前";
                }
                else if (time.Hours > 0)
                {
                    return $"数据量较大，请耐心等待。上次更新：{time.Hours}小时前";
                }
                else if (time.Minutes > 0)
                {
                    return $"数据量较大，请耐心等待。上次更新：{time.Minutes}分钟前";
                }
                else
                {
                    return $"数据量较大，请耐心等待。上次更新：刚刚";
                }
            }
            else
            {
                return $"数据量较大，请耐心等待";
            }
        }
        catch
        {
            return $"数据量较大，请耐心等待";
        }
    }


    /// <summary>
    /// 使用 AppCenter 上传事件日志
    /// </summary>
    [ObservableProperty]
    private bool _AgreeTrackEventByAppCenter = AppSetting.GetValue<bool>(SettingKeys.AgreeTrackEventByAppCenter);
    partial void OnAgreeTrackEventByAppCenterChanged(bool value)
    {
        AppSetting.SetValue(SettingKeys.AgreeTrackEventByAppCenter, value);
        OperationHistory.AddToDatabase("AgreeTrackEventByAppCenter", value.ToString());
        Microsoft.AppCenter.Analytics.Analytics.TrackEvent("AgreeTrackEventByAppCenter", new Dictionary<string, string> { ["Agree"] = value.ToString() });
    }



    #endregion


}
