using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Numerics;
using System.Reflection;
using Windows.System;
using Windows.UI.StartScreen;
using Xunkong.Hoyolab;
using Xunkong.Hoyolab.Account;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class MainPage : Page
{


    private readonly HoyolabService _hoyolabService;

    private readonly XunkongApiService _xunkongApiService;


    private string AppName => XunkongEnvironment.AppName;


    public static MainPage Current { get; private set; }



    public MainPage()
    {
        Current = this;
        this.InitializeComponent();
        MainWindow.Current.SetTitleBar(_appTitleBar);
        _hoyolabService = ServiceProvider.GetService<HoyolabService>()!;
        _xunkongApiService = ServiceProvider.GetService<XunkongApiService>()!;
        Loaded += MainPage_Loaded;
    }





    #region Navigation Display


    private void _NavigationView_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        Thickness currMargin = _appTitleBar.Margin;
        double left = 0;
        if (sender.DisplayMode == NavigationViewDisplayMode.Minimal)
        {
            left = sender.CompactPaneLength * 2;
            _appTitleBar.Margin = new Thickness(left, currMargin.Top, currMargin.Right, currMargin.Bottom);
            _NavigationView.IsPaneToggleButtonVisible = true;
        }
        else
        {
            left = sender.CompactPaneLength;
            _appTitleBar.Margin = new Thickness(left, currMargin.Top, currMargin.Right, currMargin.Bottom);
            _NavigationView.IsPaneToggleButtonVisible = false;
        }
        UpdateAppTitleMargin(sender);
    }


    private void _NavigationView_PaneOpening(NavigationView sender, object args)
    {
        UpdateAppTitleMargin(sender);
        _Border_AccountImage.Width = 44;
        _Border_AccountImage.Height = 44;
        _Border_AccountImage.Margin = new Thickness(16, 0, 0, 0);
    }

    private void _NavigationView_PaneClosing(NavigationView sender, NavigationViewPaneClosingEventArgs args)
    {
        UpdateAppTitleMargin(sender);
        _Border_AccountImage.Width = 40;
        _Border_AccountImage.Height = 40;
        _Border_AccountImage.Margin = new Thickness(4, 4, 4, 0);
    }

    private void UpdateAppTitleMargin(NavigationView sender)
    {
        const int smallLeftIndent = 4, largeLeftIndent = 24;

        AppTitle.TranslationTransition = new Vector3Transition();

        if ((sender.DisplayMode == NavigationViewDisplayMode.Expanded && sender.IsPaneOpen) ||
                 sender.DisplayMode == NavigationViewDisplayMode.Minimal)
        {
            AppTitle.Translation = new System.Numerics.Vector3(smallLeftIndent, 0, 0);
        }
        else
        {
            AppTitle.Translation = new System.Numerics.Vector3(largeLeftIndent, 0, 0);
        }
    }


    private void _Border_AccountImage_Tapped(object sender, TappedRoutedEventArgs e)
    {
        _NavigationView.IsPaneOpen = !_NavigationView.IsPaneOpen;
        AppSetting.SetValue(SettingKeys.NavigationViewPaneClose, !_NavigationView.IsPaneOpen);
    }


    #endregion


    #region Natigation Natigate


    private readonly Dictionary<string, Type> pageTypeDic = new();


    private void _NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        try
        {
            if (args.InvokedItemContainer.IsSelected)
            {
                return;
            }
            if (args.IsSettingsInvoked)
            {
                _MainPageFrame.Navigate(typeof(SettingPage));
                OperationHistory.AddToDatabase("Navigate", "SettingPage");
            }
            else
            {
                if (args.InvokedItemContainer.DataContext is WebToolItem webToolItem)
                {
                    _MainPageFrame.Navigate(typeof(WebToolPage), webToolItem);
                    OperationHistory.AddToDatabase("Navigate", "WebToolPage", webToolItem);
                }
                else
                {
                    var tag = args.InvokedItemContainer.Tag as string;
                    if (string.IsNullOrWhiteSpace(tag))
                    {
                        return;
                    }
                    if (tag == "Help")
                    {
                        _MainPageFrame.Navigate(typeof(WebViewPage), "https://xunkong.cc/help/xunkong/");
                        OperationHistory.AddToDatabase("Navigate", "HelpPage");
                    }
                    else
                    {
                        if (pageTypeDic.TryGetValue(tag, out var type))
                        {
                            _MainPageFrame.Navigate(type);
                        }
                        else
                        {
                            var asm = typeof(MainPage).Assembly;
                            type = asm.GetType($"Xunkong.Desktop.Pages.{tag}");
                            if (type is not null)
                            {
                                pageTypeDic.TryAdd(tag, type);
                                _MainPageFrame.Navigate(type);
                            }
                        }
                        OperationHistory.AddToDatabase("Navigate", tag);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex, $"导肮错误 {args.InvokedItemContainer.Tag}");
            Logger.Error(ex, $"导肮错误 {args.InvokedItemContainer.Tag}");
        }
    }


    private void _NavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        GoBack();
    }


    private void GoBack()
    {
        try
        {
            if (_MainPageFrame.CanGoBack)
            {
                _MainPageFrame.GoBack();
                _NavigationView.SelectedItem = null;
            }
        }
        catch { }
    }



    private void _MainPageFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        NotificationProvider.Error(e.Exception, "导航失败");
        Logger.Error(e.Exception, "导航失败");
        e.Handled = true;
    }



    public void Navigate(Type sourcePageType, object? param = null, NavigationTransitionInfo? infoOverride = null)
    {
        if (param is null)
        {
            _MainPageFrame.Navigate(sourcePageType);
        }
        else if (infoOverride is null)
        {
            _MainPageFrame.Navigate(sourcePageType, param);
        }
        else
        {
            _MainPageFrame.Navigate(sourcePageType, param, infoOverride);
        }
    }



    private void _MainPageFrame_Navigated(object sender, NavigationEventArgs e)
    {
        try
        {
            if (e.NavigationMode is NavigationMode.New)
            {
                var type = e.SourcePageType.Name;
                if (type is "SettingPage" or "WebToolPage" or "HelpPage")
                {
                    return;
                }
                if (pageTypeDic.ContainsKey(type))
                {
                    return;
                }
                OperationHistory.AddToDatabase("Navigate", type);
            }
        }
        catch { }
    }


    /// <summary>
    /// Esc 键，后退
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void Page_ProcessKeyboardAccelerators(UIElement sender, ProcessKeyboardAcceleratorEventArgs args)
    {
        if (args.Key == VirtualKey.Escape && args.Modifiers == VirtualKeyModifiers.None)
        {
            GoBack();
            args.Handled = true;
        }
    }


    /// <summary>
    /// 鼠标中键，后退
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Page_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (e.GetCurrentPoint(null).Properties.IsMiddleButtonPressed)
        {
            isMiddleButtonPressed = true;
            e.Handled = true;
        }
        else
        {
            isMiddleButtonPressed = false;
        }
    }


    private bool isMiddleButtonPressed;


    /// <summary>
    /// 鼠标中键，后退
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Page_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (isMiddleButtonPressed)
        {
            GoBack();
            e.Handled = true;
            isMiddleButtonPressed = false;
        }
    }



    #endregion




    [ObservableProperty]
    private HoyolabUserInfo? hoyolabUserInfo;


    [ObservableProperty]
    private GenshinRoleInfo? genshinRoleInfo;

    [ObservableProperty]
    private ObservableCollection<GenshinRoleInfo> genshinRoleInfoList;

    [ObservableProperty]
    private List<GameRecordItem> gameRecordList;




    private void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (AppSetting.TryGetValue<Version>(SettingKeys.LastVersion, out var version) && XunkongEnvironment.AppVersion == version)
        {
            _NavigationView.SelectedItem = _NaviItem_HomePage;
            _MainPageFrame.Navigate(typeof(HomePage));
        }
        else
        {
            OperationHistory.AddToDatabase("UpdateVersion", XunkongEnvironment.AppVersion.ToString());
            _MainPageFrame.Navigate(typeof(UpdateContentPage));
        }
        if (AppSetting.TryGetValue<bool>(SettingKeys.NavigationViewPaneClose, out var isClosed))
        {
            _NavigationView.IsPaneOpen = !isClosed;
        }
        InitializeJumpList();
        RefreshAllAcount();
        GetGenshinData();
        InitializeNavigationWebToolItem(true);
        // todo 壁纸浏览页
    }


    /// <summary>
    /// 初始化导航栏网页工具
    /// </summary>
    public async void InitializeNavigationWebToolItem(bool delay = false)
    {
        if (delay)
        {
            await Task.Delay(600);
        }
        const string WEBTOOL = "WebTool";
        try
        {
            var removing = _NavigationView.MenuItems.Where(x => (x as FrameworkElement)?.Tag as string == WEBTOOL).ToList();
            foreach (var item in removing)
            {
                _NavigationView.MenuItems.Remove(item);
            }
            List<WebToolItem>? list = null;
            await Task.Run(() =>
            {
                using var dapper = DatabaseProvider.CreateConnection();
                list = dapper.Query<WebToolItem>("SELECT * FROM WebToolItem ORDER BY [Order]").ToList();
            });
            if (list?.Any() ?? false)
            {
                _NavigationView.MenuItems.Add(new NavigationViewItemSeparator { Tag = WEBTOOL });
                foreach (var item in list)
                {
                    _NavigationView.MenuItems.Add(new NavigationViewItem
                    {
                        Icon = new BitmapIcon { UriSource = new Uri(item.Icon ?? ""), ShowAsMonochrome = false },
                        Content = item.Title,
                        DataContext = item,
                        Tag = WEBTOOL,
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "加载网页小工具");
        }
    }


    /// <summary>
    /// 获取原神数据
    /// </summary>
    public void GetGenshinData()
    {
        Task.Run(async () =>
        {
            try
            {
                await _xunkongApiService.GetAllGenshinDataFromServerAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取原神数据");
            }
        }).ConfigureAwait(false);
    }







    /// <summary>
    /// 刷新所有账号
    /// </summary>
    [RelayCommand]
    private void RefreshAllAcount()
    {
        try
        {
            HoyolabUserInfo = _hoyolabService.GetLastSelectedOrFirstHoyolabUserInfo();
            GenshinRoleInfo = _hoyolabService.GetLastSelectedOrFirstGenshinRoleInfo();
            GenshinRoleInfoList = new(_hoyolabService.GetGenshinRoleInfoList());
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "获取所有账号信息");
        }
        Task.Run(async () =>
        {
            try
            {
                await _hoyolabService.UpdateAllAccountsAsync().ConfigureAwait(false);
                if (!AppSetting.GetValue<bool>(SettingKeys.SignInAllAccountsWhenStartUpApplication))
                {
                    return;
                }
                var roles = _hoyolabService.GetGenshinRoleInfoList();
                foreach (var role in roles)
                {
                    try
                    {
                        await _hoyolabService.SignInAsync(role).ConfigureAwait(false);
                    }
                    catch (HoyolabException e)
                    {
                        NotificationProvider.Error(e, $"签到 {role.Nickname}");
                        Logger.Error(e, $"签到 {role.Nickname}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "刷新所有账号信息或签到");
            }
        }).ConfigureAwait(false);
    }



    private async void InitializeJumpList()
    {
        try
        {
            var jumpList = await JumpList.LoadCurrentAsync();
            jumpList.Items.Clear();
            var item1 = JumpListItem.CreateWithArguments("StartGame", "启动游戏");
            item1.Logo = new Uri("ms-appx:///Assets/Logos/StoreLogo.png");
            jumpList.Items.Add(item1);
            await jumpList.SaveAsync();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "JumpList");
        }
    }


    [RelayCommand]
    private void ShowChangeAccountFlyout()
    {
        _Flyout_AddAccount.ShowAt(_Button_AddAccount);
    }

    /// <summary>
    /// 账号变更
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _ListView_Account_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (_ListView_Account.SelectedItem is GenshinRoleInfo role)
            {
                GenshinRoleInfo = role;
                AppSetting.SetValue(SettingKeys.LastSelectGameRoleUid, role.Uid);
                var user = _hoyolabService.GetHoyolabUserInfoFromDatabaseByCookie(role.Cookie!);
                if (user is not null)
                {
                    HoyolabUserInfo = user;
                    AppSetting.SetValue(SettingKeys.LastSelectUserInfoUid, user.Uid);
                }
                _Flyout_ChangeAccount.Hide();
                WeakReferenceMessenger.Default.Send(new SelectedGameRoleChangedMessage { Uid = role.Uid, Role = role });
            }
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex, "切换账号");
            Logger.Error(ex, "切换账号");
        }
    }


    /// <summary>
    /// 导航到米游社登录页面
    /// </summary>
    [RelayCommand]
    private void NavigateToLoginPage()
    {
        _MainPageFrame.Navigate(typeof(LoginPage));
    }



    [RelayCommand]
    private async Task GetGameRecordAsync()
    {
        try
        {
            if (GenshinRoleInfo != null)
            {
                var record = await _hoyolabService.GetGameRecordSummaryAsync(GenshinRoleInfo);
                GameRecordList = record.PlayerStat.GetType()
                                                  .GetProperties()
                                                  .Select(x => new GameRecordItem(x.GetCustomAttribute<DescriptionAttribute>()?.Description!, x.GetValue(record.PlayerStat)?.ToString()!))
                                                  .ToList();
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }


    public record GameRecordItem
    {
        public GameRecordItem(string description, string value)
        {
            Description = description;
            Value = value;
        }

        public string Description { get; set; }

        public string Value { get; set; }
    }



    /// <summary>
    /// 手动添加cookie
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task AddCookieAsync()
    {
        var text = new TextBlock();
        text.Inlines.Add(new Run { Text = "仅支持国服米游社，" });
        var link = new Hyperlink { NavigateUri = new("https://xunkong.cc/help/xunkong/account.html"), UnderlineStyle = UnderlineStyle.None };
        var linkeText = new Run { Text = "如何获取 Cookie" };
        link.Inlines.Add(linkeText);
        text.Inlines.Add(link);
        text.Inlines.Add(new LineBreak());
        text.Inlines.Add(new Run { Text = "需要包含 cookie_token_v2 值，否则会出现 HoyolabException (-100) 错误" });

        var stackPanel = new StackPanel { Spacing = 12 };
        stackPanel.Children.Add(text);
        var textBox = new TextBox();
        stackPanel.Children.Add(textBox);
        var dialog = new ContentDialog
        {
            Title = "输入Cookie",
            Content = stackPanel,
            PrimaryButtonText = "确认",
            SecondaryButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = MainWindow.Current.XamlRoot,
            RequestedTheme = MainWindow.Current.ActualTheme,
        };
        if (await dialog.ShowWithZeroMarginAsync() is ContentDialogResult.Primary)
        {
            var cookie = textBox.Text;
            if (string.IsNullOrWhiteSpace(cookie))
            {
                return;
            }
            try
            {
                await AddCookieAsync(cookie);
            }
            catch (Exception ex)
            {
                NotificationProvider.Error(ex, "获取账号信息");
                Logger.Error(ex, "获取账号信息");
            }
        }
    }



    public async Task AddCookieAsync(string cookie)
    {
        var user = await _hoyolabService.GetHoyolabUserInfoAsync(cookie);
        var roles = await _hoyolabService.GetGenshinRoleInfoListAsync(cookie);
        HoyolabUserInfo = user;
        GenshinRoleInfo = roles.FirstOrDefault();
        AppSetting.SetValue(SettingKeys.LastSelectUserInfoUid, user.Uid);
        if (GenshinRoleInfo is not null)
        {
            AppSetting.SetValue(SettingKeys.LastSelectGameRoleUid, GenshinRoleInfo.Uid);
        }
        GenshinRoleInfoList = new(_hoyolabService.GetGenshinRoleInfoList());
    }



    /// <summary>
    /// 固定磁贴到开始菜单
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void _Button_PinTile_Click(object sender, RoutedEventArgs e)
    {
        //if (Environment.OSVersion.Version >= new Version("10.0.22000.0"))
        //{
        //    NotificationProvider.Warning("您的操作系统不支持开始菜单磁贴", 3000);
        //    return;
        //}
        try
        {
            if (((FrameworkElement)sender).DataContext is GenshinRoleInfo role)
            {
                var note = await _hoyolabService.GetDailyNoteAsync(role);
                if (note is not null)
                {
                    var result = await SecondaryTileProvider.RequestPinTileAsync(note);
                    if (result)
                    {
                        TaskSchedulerService.RegisterForRefreshTile(result);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex, "固定磁贴到开始菜单");
            Logger.Error(ex, "固定磁贴到开始菜单");
        }
    }







    /// <summary>
    /// 复制所选账号的cookie
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _Button_CopyCookie_Click(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is GenshinRoleInfo role)
        {
            try
            {
                ClipboardHelper.SetText(role.Cookie);
                NotificationProvider.Success("已复制", 1500);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "复制 cookie");
                NotificationProvider.Error(ex, "复制 cookie");
            }

        }
    }



    /// <summary>
    /// 删除所选账号
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void _Button_DeleteAccount_Click(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is GenshinRoleInfo role)
        {
            var text = new TextBlock();
            text.Inlines.Add(new Run { Text = $"删除 {role.Nickname} ？", FontSize = 16 });
            text.Inlines.Add(new LineBreak());
            text.Inlines.Add(new Run { Text = "所有 Cookie 相同的账号也将被删除", FontSize = 12 });
            var dialog = new ContentDialog
            {
                Title = $"删除 {role.Nickname} ？",
                Content = "所有 Cookie 相同的账号也将被删除",
                PrimaryButtonText = "确认",
                SecondaryButtonText = "取消",
                DefaultButton = ContentDialogButton.Secondary,
                XamlRoot = MainWindow.Current.XamlRoot,
                RequestedTheme = MainWindow.Current.ActualTheme,
            };
            if (await dialog.ShowWithZeroMarginAsync() is ContentDialogResult.Primary)
            {
                try
                {
                    _hoyolabService.DeleteHoyolabUserInfo(role.Cookie!);
                    _hoyolabService.DeleteGenshinRoleInfo(role.Cookie!);
                    HoyolabUserInfo = _hoyolabService.GetLastSelectedOrFirstHoyolabUserInfo();
                    GenshinRoleInfo = _hoyolabService.GetLastSelectedOrFirstGenshinRoleInfo();
                    GenshinRoleInfoList = new(_hoyolabService.GetGenshinRoleInfoList());
                }
                catch (Exception ex)
                {
                    NotificationProvider.Error(ex, "删除账号");
                    Logger.Error(ex, "删除账号");
                }
            }
        }
    }

    private void Button_ChangeTheme_Tapped(object sender, TappedRoutedEventArgs e)
    {
        try
        {
            var point = _Border_AccountImage.TransformToVisual(this).TransformPoint(new Windows.Foundation.Point(0, 0));
            var center = new Vector2((float)(point.X + _Border_AccountImage.ActualWidth / 2), (float)(point.Y + _Border_AccountImage.ActualHeight / 2));
            if (ActualTheme is ElementTheme.Light)
            {
                MainWindow.Current.ChangeApplicationTheme(2, center);
            }
            else
            {
                MainWindow.Current.ChangeApplicationTheme(1, center);
            }
            e.Handled = true;
            OperationHistory.AddToDatabase("ChangeTheme", "ClickButton");
        }
        catch { }
    }


}



file class DescriptionAttributeExtension
{
    public static string? GetDescription(object obj)
    {
        var descriptionAttribute = obj.GetType().GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault();
        return (descriptionAttribute as DescriptionAttribute)?.Description;
    }
}