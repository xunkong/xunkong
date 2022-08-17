using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using Windows.UI.StartScreen;
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
        WeakReferenceMessenger.Default.Register<InitializeNavigationWebToolItemMessage>(this, (_, e) => InitializeNavigationWebToolItem());
        Loaded += MainPage_Loaded;
    }


    // todo 再次简化主页面元素



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
    }


    #endregion


    #region Natigation Natigate


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
            }
            else
            {
                if (args.InvokedItemContainer.DataContext is WebToolItem webToolItem)
                {
                    _MainPageFrame.Navigate(typeof(WebToolPage), webToolItem);
                }
                else
                {
                    var tag = args.InvokedItemContainer.Tag as string;
                    var asm = typeof(MainPage).Assembly;
                    var type = asm.GetType($"Xunkong.Desktop.Pages.{tag}");
                    if (type is not null)
                    {
                        _MainPageFrame.Navigate(type);
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
        if (_MainPageFrame.CanGoBack)
        {
            _MainPageFrame.GoBack();
            _NavigationView.SelectedItem = null;
        }
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



    #endregion




    [ObservableProperty]
    private HoyolabUserInfo? hoyolabUserInfo;


    [ObservableProperty]
    private GenshinRoleInfo? genshinRoleInfo;

    [ObservableProperty]
    private ObservableCollection<GenshinRoleInfo> genshinRoleInfoList;




    private void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
        _NavigationView.SelectedItem = _NaviItem_HomePage;
        _MainPageFrame.Navigate(typeof(HomePage));
        InitializeNavigationWebToolItem();
        GetGenshinDataAsync();
        RefreshAllAcountAsync();
        SignInAsync();
        InitializeJumpList();
        // todo 壁纸浏览页
    }


    /// <summary>
    /// 初始化导航栏网页工具
    /// </summary>
    private async void InitializeNavigationWebToolItem()
    {
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
            NotificationProvider.Error(ex, "加载网页小工具");
            Logger.Error(ex, "加载网页小工具");
        }
    }


    /// <summary>
    /// 获取原神数据
    /// </summary>
    public async void GetGenshinDataAsync()
    {
        try
        {
            await _xunkongApiService.GetAllGenshinDataFromServerAsync();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "获取原神数据");
        }
    }






    /// <summary>
    /// 刷新所有账号
    /// </summary>
    [RelayCommand]
    private async void RefreshAllAcountAsync()
    {
        await Task.Run(async () =>
        {
            try
            {
                var selectedUser = _hoyolabService.GetLastSelectedOrFirstHoyolabUserInfo();
                var selectedRole = _hoyolabService.GetLastSelectedOrFirstGenshinRoleInfo();
                var roles = _hoyolabService.GetGenshinRoleInfoList().ToList();
                DispatcherQueue.TryEnqueue(() =>
                {
                    HoyolabUserInfo = selectedUser;
                    GenshinRoleInfo = selectedRole;
                    GenshinRoleInfoList = new(roles);
                });
                if (UserSetting.GetValue(SettingKeys.ResinAndHomeCoinNotificationWhenStartUp, false, false))
                {
                    foreach (var role in roles)
                    {
                        var note = await _hoyolabService.GetDailyNoteAsync(role);
                        if (note != null)
                        {
                            var text = (note.IsResinFull, note.IsHomeCoinFull) switch
                            {
                                (true, true) => $"{role.Nickname} 的 原粹树脂 和 洞天宝钱 已满",
                                (true, false) => $"{role.Nickname} 的 原粹树脂 已满",
                                (false, true) => $"{role.Nickname} 的 洞天宝钱 已满",
                                (false, false) => "",
                            };
                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                // 方法内部会切换为UI线程
                                NotificationProvider.Success("注意了", text, 8000);
                            }
                        }
                    }
                }
                await _hoyolabService.UpdateAllAccountsAsync();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "刷新所有账号信息");
            }
        });
    }


    /// <summary>
    /// 签到
    /// </summary>
    private async void SignInAsync()
    {
        try
        {
            if (!UserSetting.GetValue<bool>(SettingKeys.SignInAllAccountsWhenStartUpApplication))
            {
                return;
            }
            var roles = _hoyolabService.GetGenshinRoleInfoList();
            foreach (var role in roles)
            {
                try
                {
                    await _hoyolabService.SignInAsync(role);
                }
                catch (Exception e)
                {
                    NotificationProvider.Error(e, $"签到 {role.Nickname}");
                    Logger.Error(e, $"签到 {role.Nickname}");
                }
            }
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex, "签到");
            Logger.Error(ex, "签到");
        }
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
        catch { }
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
                UserSetting.TrySetValue(SettingKeys.LastSelectGameRoleUid, role.Uid);
                var user = _hoyolabService.GetHoyolabUserInfoFromDatabaseByCookie(role.Cookie!);
                if (user is not null)
                {
                    HoyolabUserInfo = user;
                    UserSetting.TrySetValue(SettingKeys.LastSelectUserInfoUid, user.Uid);
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
        var action = new Action<string>(async (cookie) =>
        {
            try
            {
                var user = await _hoyolabService.GetHoyolabUserInfoAsync(cookie);
                var roles = await _hoyolabService.GetGenshinRoleInfoListAsync(cookie);
                HoyolabUserInfo = user;
                GenshinRoleInfo = roles.FirstOrDefault();
                UserSetting.TrySetValue(SettingKeys.LastSelectUserInfoUid, user.Uid);
                if (GenshinRoleInfo is not null)
                {
                    UserSetting.TrySetValue(SettingKeys.LastSelectGameRoleUid, GenshinRoleInfo.Uid);
                }
                GenshinRoleInfoList = new(_hoyolabService.GetGenshinRoleInfoList());
                NotificationProvider.Success("已添加账号");
            }
            catch (Exception ex)
            {
                NotificationProvider.Error(ex, "导航到米游社登录页面");
                Logger.Error(ex, "导航到米游社登录页面");
            }
        });
        _MainPageFrame.Navigate(typeof(LoginPage), action);
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
        var link = new Hyperlink { NavigateUri = new("https://go.xunkong.cc/how-to-get-cookie"), UnderlineStyle = UnderlineStyle.None };
        var linkeText = new Run { Text = "如何获取 Cookie" };
        link.Inlines.Add(linkeText);
        text.Inlines.Add(link);
        text.Inlines.Add(new LineBreak());
        text.Inlines.Add(new Run { Text = "需要包含 cookie_token 值，否则会出现 HoyolabException (-100) 错误" });

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
                var user = await _hoyolabService.GetHoyolabUserInfoAsync(cookie);
                var roles = await _hoyolabService.GetGenshinRoleInfoListAsync(cookie);
                HoyolabUserInfo = user;
                GenshinRoleInfo = roles.FirstOrDefault();
                UserSetting.TrySetValue(SettingKeys.LastSelectUserInfoUid, user.Uid);
                if (GenshinRoleInfo is not null)
                {
                    UserSetting.TrySetValue(SettingKeys.LastSelectGameRoleUid, GenshinRoleInfo.Uid);
                }
                GenshinRoleInfoList = new(_hoyolabService.GetGenshinRoleInfoList());
            }
            catch (Exception ex)
            {
                NotificationProvider.Error(ex, "获取账号信息");
                Logger.Error(ex, "获取账号信息");
            }
        }
    }



    /// <summary>
    /// 固定磁贴到开始菜单
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void _Button_PinTile_Click(object sender, RoutedEventArgs e)
    {
        if (Environment.OSVersion.Version >= new Version("10.0.22000.0"))
        {
            NotificationProvider.Warning("您的操作系统不支持开始菜单磁贴", 3000);
            return;
        }
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
                        await SecondaryTileProvider.RegisterTaskAsync();
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
            ClipboardHelper.SetText(role.Cookie);
            NotificationProvider.Success("已复制", 1500);
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




}
