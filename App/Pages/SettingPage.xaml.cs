using AngleSharp;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Collections.ObjectModel;
using System.Net.Http;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using WinRT.Interop;
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

    public string AppName => XunkongEnvironment.AppName;

    public string AppVersion => XunkongEnvironment.AppVersion.ToString();

    public string UserDataPath => XunkongEnvironment.UserDataPath;



    public SettingPage()
    {
        this.InitializeComponent();
        _httpClient = ServiceProvider.GetService<HttpClient>()!;
        _xunkongApiService = ServiceProvider.GetService<XunkongApiService>()!;
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
        MainWindow.Current.SetFullWindowContent(new ImageViewer { Source = "ms-appx:///Assets/Images/102203689_p0.jpg" });
    }

    /// <summary>
    /// 好评
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private async void Hyperlink_Click(Microsoft.UI.Xaml.Documents.Hyperlink sender, Microsoft.UI.Xaml.Documents.HyperlinkClickEventArgs args)
    {
        await Launcher.LaunchUriAsync(new("ms-windows-store://review/?ProductId=9N2SVG0JMT12"));
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
            Uri uri;
            if (XunkongEnvironment.IsStoreVersion)
            {
                uri = new("ms-windows-store://pdp/?productid=9N2SVG0JMT12");
            }
            else
            {
                uri = new("https://github.com/xunkong/xunkong/releases");
            }
            await Launcher.LaunchUriAsync(uri);
            OperationHistory.AddToDatabase("CheckUpdate", XunkongEnvironment.AppVersion.ToString());
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
            var folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");
            InitializeWithWindow.Initialize(folderPicker, MainWindow.Current.HWND);
            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder is not null)
            {
                WallpaperSaveFolder = folder.Path;
                AppSetting.SetValue(SettingKeys.WallpaperSaveFolder, folder.Path);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "修改壁纸保存位置");
            NotificationProvider.Error(ex, "修改壁纸保存位置");
        }
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
            var dialog = new FileOpenPicker();
            dialog.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            dialog.FileTypeFilter.Add(".exe");
            InitializeWithWindow.Initialize(dialog, MainWindow.Current.HWND);
            var file = await dialog.PickSingleFileAsync();
            if (file != null)
            {
                var path = file.Path;
                if (path.EndsWith(".exe"))
                {
                    switch (server)
                    {
                        case "0":
                            AppSetting.SetValue(SettingKeys.GameExePathCN, path);
                            GameExePathCN = file.Path;
                            break;
                        case "1":
                            AppSetting.SetValue(SettingKeys.GameExePathGlobal, path);
                            GameExePathGlobal = file.Path;
                            break;
                        case "2":
                            AppSetting.SetValue(SettingKeys.GameExePathCNCloud, path);
                            GameExePathCNCloud = file.Path;
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



    #region WebTool


    /// <summary>
    /// 快捷方式列表
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<WebToolItem> _WebToolItemList;

    /// <summary>
    /// 选中的快捷方式
    /// </summary>
    [ObservableProperty]
    private WebToolItem? _SelectedWebToolItem;


    /// <summary>
    /// 展开快捷方式栏
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void _Expander_WebTool_Expanding(Expander sender, ExpanderExpandingEventArgs args)
    {
        LoadWebToolItems();
    }

    /// <summary>
    /// 关闭快捷方式栏
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void _Expander_WebTool_Collapsed(Expander sender, ExpanderCollapsedEventArgs args)
    {
        SelectedWebToolItem = null;
    }

    /// <summary>
    /// 加载网页快捷方式
    /// </summary>
    public void LoadWebToolItems()
    {
        try
        {
            using var dapper = DatabaseProvider.CreateConnection();
            var list = dapper.Query<WebToolItem>("SELECT * FROM WebToolItem ORDER BY [Order];");
            WebToolItemList = new(list);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "无法加载网页快捷方式的数据");
            NotificationProvider.Error(ex, "无法加载网页快捷方式的数据");
        }
    }


    /// <summary>
    /// 添加新的网页快捷方式
    /// </summary>
    [RelayCommand]
    private void AddWebToolItem()
    {
        try
        {
            var newItem = new WebToolItem();
            if (WebToolItemList is null)
            {
                WebToolItemList = new();
            }
            WebToolItemList.Add(newItem);
            SelectedWebToolItem = newItem;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "添加新的网页快捷方式");
        }

    }


    /// <summary>
    /// 删除选择的网页快捷方式，但未保存至数据库
    /// </summary>
    [RelayCommand]
    private void DeleteSelectedWebToolItem()
    {
        try
        {
            if (SelectedWebToolItem is not null)
            {
                var index = WebToolItemList.IndexOf(SelectedWebToolItem);
                WebToolItemList.Remove(SelectedWebToolItem);
                if (WebToolItemList.Count < index + 1)
                {
                    SelectedWebToolItem = WebToolItemList.LastOrDefault();
                }
                else
                {
                    SelectedWebToolItem = WebToolItemList.ElementAt(index);
                }
            }
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex, "删除选择的网页快捷方式");
        }

    }


    /// <summary>
    /// 关闭网页快捷方式编辑栏
    /// </summary>
    [RelayCommand]
    private void CloseEditWebToolGrid()
    {
        SelectedWebToolItem = null;
    }


    /// <summary>
    /// 获取网页的图标
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task GetTitleAndIconByUrlAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedWebToolItem?.Url))
        {
            return;
        }
        var url = SelectedWebToolItem.Url;
        try
        {
            var uri = new Uri(url);
            var baseUri = new Uri($"{uri.Scheme}://{uri.Host}");
            var html = await _httpClient.GetStringAsync(url);
            var context = BrowsingContext.New(Configuration.Default);
            var dom = await context.OpenAsync(x => { x.Content(html); x.Address(url); });
            SelectedWebToolItem.Title = dom.Title;
            var iconList = dom.Head?.Children.Where(x => x.Attributes["rel"]?.Value.Contains("icon") ?? false);
            var href = iconList?.FirstOrDefault()?.Attributes["href"]?.Value;
            if (string.IsNullOrWhiteSpace(href))
            {
                SelectedWebToolItem.Icon = new Uri(baseUri, "favicon.ico").ToString();
            }
            else
            {
                SelectedWebToolItem.Icon = new Uri(baseUri, href).ToString();
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "获取网页的图标");
            NotificationProvider.Error(ex, "获取网页的图标");
        }
    }




    /// <summary>
    /// 保存网页快捷方式的修改
    /// </summary>
    [RelayCommand]
    private void SaveWebToolItem()
    {
        try
        {
            var list = WebToolItemList.Where(x => !string.IsNullOrWhiteSpace(x.Url)).ToList();
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Order = i;
            }
            using var dapper = DatabaseProvider.CreateConnection();
            using var t = dapper.BeginTransaction();
            dapper.Execute("DELETE FROM WebToolItem WHERE TRUE;", t);
            dapper.Execute("INSERT INTO WebToolItem (Title, Icon, [Order], Url, JavaScript) VALUES (@Title, @Icon, @Order, @Url, @JavaScript);", list, t);
            t.Commit();
            WebToolItemList = new(list);
            NotificationProvider.Success("保存成功");
            MainPage.Current.InitializeNavigationWebToolItem();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "保存网页快捷方式");
            NotificationProvider.Error(ex, "保存网页快捷方式");
        }
    }




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



    #region Data



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
    /// 已缓存 xxx MB
    /// </summary>
    [ObservableProperty]
    private string _cachedFileSizeString;


    /// <summary>
    /// 计算已缓存文件的大小
    /// </summary>
    private void ComputeCachedFileTotalSize()
    {
        try
        {
            long totalSize = 0;
            var folder = ApplicationData.Current.TemporaryFolder.Path;
            if (Directory.Exists(folder))
            {
                var files = new DirectoryInfo(folder).GetFiles("*", SearchOption.AllDirectories);
                totalSize += files.Sum(x => x.Length);
            }

            if (totalSize > 0)
            {

                CachedFileSizeString = $"已缓存 {(double)totalSize / (1 << 20):F2} MB";
            }
            else
            {
                CachedFileSizeString = $"已缓存 0 MB";
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "计算已缓存文件的大小");
            CachedFileSizeString = "无法计算已缓存文件的大小";
        }
    }


    /// <summary>
    /// 清除缓存
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task ClearCacheAsync()
    {
        try
        {
            var folder = ApplicationData.Current.TemporaryFolder.Path;

            if (Directory.Exists(folder))
            {
                await Task.Run(() =>
                {
                    Directory.Delete(folder, true);
                    Directory.CreateDirectory(folder);
                    Directory.CreateDirectory(XunkongCache.Instance.GetCacheFolderAsync().GetAwaiter().GetResult().Path);
                });
            }

            NotificationProvider.Success($"完成");
            OperationHistory.AddToDatabase("ClearCache");
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex, "清除缓存");
            Logger.Error(ex, "清除缓存");
        }
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
            NotificationProvider.Success("完成", "本地的原神数据已是最新版本");
            OperationHistory.AddToDatabase("UpdateGenshinData");
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





    #endregion



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
    /// 在主页不显示实时便笺
    /// </summary>
    [ObservableProperty]
    private bool _DisableDailyNotesInHomePage = AppSetting.GetValue<bool>(SettingKeys.DisableDailyNotesInHomePage);
    partial void OnDisableDailyNotesInHomePageChanged(bool value)
    {
        AppSetting.SetValue(SettingKeys.DisableDailyNotesInHomePage, value);
    }


}
