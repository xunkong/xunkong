// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.System;
using Xunkong.GenshinData.Character;
using Xunkong.GenshinData.Weapon;
using Xunkong.Hoyolab;
using Xunkong.Hoyolab.Wishlog;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class WishlogSummaryPage2 : Page
{


    private readonly WishlogService _wishlogService;

    private readonly XunkongApiService _xunkongApiService;

    private readonly ProxyService _proxyService;

    private readonly ImmutableList<string> ColorSet = new[] { "#5470C6", "#91CC75", "#FAC858", "#EE6666", "#73C0DE", "#3BA272", "#FC8452", "#9A60B4", "#EA7CCC" }.ToImmutableList();



    static WishlogSummaryPage2()
    {
        TypeAdapterConfig<CharacterInfo, WishlogSummaryPage_UpItem>.NewConfig().Map(dest => dest.Icon, src => src.FaceIcon);
        TypeAdapterConfig<WeaponInfo, WishlogSummaryPage_UpItem>.NewConfig().Map(dest => dest.Icon, src => src.Icon);
    }



    public WishlogSummaryPage2()
    {
        this.InitializeComponent();
        NavigationCacheMode = AppSetting.GetValue<bool>(SettingKeys.EnableNavigationCache) ? NavigationCacheMode.Enabled : NavigationCacheMode.Disabled;
        _wishlogService = ServiceProvider.GetService<WishlogService>()!;
        _xunkongApiService = ServiceProvider.GetService<XunkongApiService>()!;
        _proxyService = ServiceProvider.GetService<ProxyService>()!;
        Loaded += WishlogSummaryPage2_Loaded;
        Unloaded += WishlogSummaryPage2_Unloaded;
        StateTextHandler = new(str => StateText = str);
    }






    private Progress<string> StateTextHandler;


    [ObservableProperty]
    private string _StateText;


    private int SelectedUid
    {
        get
        {
            var uidStr = ComboBox_AllUids.SelectedItem as string;
            if (int.TryParse(uidStr, out int uid))
            {
                return uid;
            }
            else
            {
                return 0;
            }
        }
        set
        {
            ComboBox_AllUids.SelectedItem = value.ToString();
            AppSetting.SetValue(SettingKeys.LastSelectedUidInWishlogSummaryPage, value);
        }
    }

    private List<WishlogItemEx> wishlogList;


    [ObservableProperty]
    private List<WishlogItemEx>? _ItemShowList;


    [ObservableProperty]
    private ObservableCollection<string> _AllUids;


    [ObservableProperty]
    private List<WishlogSummaryPage_QueryTypeStats> _QueryTypeStats;


    [ObservableProperty]
    private List<WishlogSummaryPage_ItemThumb> _CharacterThumbs;


    [ObservableProperty]
    private List<WishlogSummaryPage_ItemThumb> _WeaponThumbs;


    [ObservableProperty]
    private List<WishlogSummaryPage_EventStats> _CharacterEventStats;


    [ObservableProperty]
    private List<WishlogSummaryPage_EventStats> _WeaponEventStats;



    private async void WishlogSummaryPage2_Loaded(object sender, RoutedEventArgs e)
    {
        await Task.Delay(60);
        _proxyService.GotWishlogUrl += _proxyService_GotWishlogUrl;
        if (QueryTypeStats is null)
        {
            InitializePageData();
        }
    }


    private void WishlogSummaryPage2_Unloaded(object sender, RoutedEventArgs e)
    {
        _proxyService.GotWishlogUrl -= _proxyService_GotWishlogUrl;
    }




    [RelayCommand]
    private void InitializePageData()
    {
        try
        {
            var uids = WishlogService.GetAllUids().ToList();
            AllUids = new(uids.Select(x => x.ToString()));

            int uid = 0;
            if (ComboBox_AllUids.SelectedItem is string { Length: > 0 } uidString)
            {
                int.TryParse(uidString, out uid);
            }
            else
            {
                uid = AppSetting.GetValue<int>(SettingKeys.LastSelectedUidInWishlogSummaryPage);
            }
            if (!uids.Contains(uid))
            {
                uid = uids.FirstOrDefault();
            }
            ComboBox_AllUids.SelectedItem = uid.ToString();
        }
        catch (Exception ex)
        {

        }
    }


    private bool loaded;


    private async Task ChangeUidAsync(int uid)
    {
        var result = await Task.Run(() => LoadWishlog(uid));
        if (!loaded)
        {
            if (wishlogList.Count == 0)
            {
                StateText = "在游戏中查看祈愿记录后，点击上方的获取记录->官服/国际服/云原神。";
            }
            else
            {
                StateText = "双击下方的角色/武器图标查看详细记录";
            }
            loaded = true;
        }
        QueryTypeStats = result.QueryTypeStats;
        CharacterThumbs = result.CharacterThumbs;
        WeaponThumbs = result.WeaponThumbs;
        CharacterEventStats = result.CharacterEventStats;
        WeaponEventStats = result.WeaponEventStats;
    }




    private LoadWishlogResult LoadWishlog(int uid)
    {

        // 卡池信息时区
        WishEventInfo.RegionType = WishlogService.UidToRegionType(uid);
        // 初始化卡池信息，所有祈愿记录
        var characters = XunkongApiService.GetGenshinData<CharacterInfo>();
        var weapons = XunkongApiService.GetGenshinData<WeaponInfo>();

        var dic_characters = characters.Where(x => !string.IsNullOrWhiteSpace(x.Name)).ToImmutableDictionary(x => x.Name!);
        var dic_weapons = weapons.Where(x => !string.IsNullOrWhiteSpace(x.Name)).ToImmutableDictionary(x => x.Name!);

        var wishlogs = WishlogService.GetWishlogItemExByUid(uid);
        wishlogList = wishlogs;
        var wishlogs_group_byname = wishlogs.GroupBy(x => x.Name).ToImmutableDictionary(x => x.Key);

        // 根据祈愿类型分类计算
        var queryTypeStats = new List<WishlogSummaryPage_QueryTypeStats>();
        var wishtypes = new List<WishType>(4) { WishType.CharacterEvent, WishType.WeaponEvent, WishType.Permanent };
        if (AppSetting.GetValue<bool>(SettingKeys.ShowNoviceWishType))
        {
            wishtypes.Add(WishType.Novice);
        }
        foreach (var type in wishtypes)
        {
            var items = wishlogs.Where(x => x.QueryType == type).OrderBy(x => x.Id).ToList();
            var totalCount = items.Count;
            if (totalCount == 0)
            {
                continue;
            }
            var rank5Count = items.Where(x => x.RankType == 5).Count();
            var rank4Count = items.Where(x => x.RankType == 4).Count();
            var startTime = items.First().Time;
            var endTime = items.Last().Time;
            var upCount = items.Where(x => x.RankType == 5 && x.IsUp).Count();
            var currentGuarantee = items.Last().RankType == 5 ? 0 : items.Last().GuaranteeIndex;
            var maxGuarantee = rank5Count == 0 ? currentGuarantee : items.Where(x => x.RankType == 5).Max(x => x.GuaranteeIndex);
            var minGuarantee = rank5Count == 0 ? currentGuarantee : items.Where(x => x.RankType == 5).Min(x => x.GuaranteeIndex);
            var guaranteeStar4 = items.Count - items.FindLastIndex(x => x.RankType == 4) - 1;
            var rank5Items = items.Where(x => x.RankType == 5).Select(x => new WishlogSummaryPage_Rank5Item(x.Name, x.GuaranteeIndex, x.Time, type == WishType.Permanent || x.IsUp)).ToList();
            rank5Items.Where(x => !x.IsUp).ToList().ForEach(x => { x.Color = "#808080"; x.Foreground = "#808080"; });
            var colors = ColorSet.OrderBy(x => Random.Shared.Next()).ToList();
            var gi = rank5Items.Where(x => x.IsUp).GroupBy(x => x.Name).OrderBy(x => x.Min(y => y.Time));
            int index = 0;
            foreach (var g in gi)
            {
                var color = colors[index];
                index = (index + 1) % colors.Count;
                g.ToList().ForEach(x => { x.Color = color; x.Foreground = color; });
            }
            var stats = new WishlogSummaryPage_QueryTypeStats(type,
                                                              totalCount,
                                                              rank5Count,
                                                              rank4Count,
                                                              startTime,
                                                              endTime,
                                                              upCount,
                                                              currentGuarantee,
                                                              maxGuarantee,
                                                              minGuarantee,
                                                              guaranteeStar4,
                                                              rank5Items,
                                                              items.LastOrDefault(x => x.RankType == 5)?.IsUp ?? false,
                                                              items.LastOrDefault(x => x.RankType == 5)?.GuaranteeIndex ?? 0);
            queryTypeStats.Add(stats);
        }



        var query_character = from g in wishlogs_group_byname
                              join c in dic_characters
                              on g.Key equals c.Key
                              select new WishlogSummaryPage_ItemThumb(g.Key, c.Value.Rarity, c.Value.Element, c.Value.WeaponType, g.Value.Count(), c.Value.FaceIcon, g.Value.Last().Time);
        var characterThumbs = query_character.OrderByDescending(x => x.Rarity).ThenByDescending(x => x.Count).ThenByDescending(x => x.LastTime).ToList();

        var query_weapon = from g in wishlogs_group_byname
                           join c in dic_weapons
                           on g.Key equals c.Key
                           select new WishlogSummaryPage_ItemThumb(g.Key, c.Value.Rarity, ElementType.None, c.Value.WeaponType, g.Value.Count(), c.Value.Icon, g.Value.Last().Time);
        var weaponThumbs = query_weapon.OrderByDescending(x => x.Rarity).ThenByDescending(x => x.Count).ThenByDescending(x => x.LastTime).ToList();


        // 根据卡池分类计算
        var eventInfos = XunkongApiService.GetGenshinData<WishEventInfo>();

        var character_groups = eventInfos.Where(x => x.QueryType == WishType.CharacterEvent).GroupBy(x => x.StartTime).ToList();
        var character_eventStats = new List<WishlogSummaryPage_EventStats>();
        foreach (var group in character_groups)
        {
            var stats = group.FirstOrDefault()?.Adapt<WishlogSummaryPage_EventStats>()!;
            character_eventStats.Add(stats);
            stats.Name = string.Join("\n", group.Select(x => x.Name));
            stats.UpItems = group.SelectMany(x => x.Rank5UpItems).Join(dic_characters, str => str, dic => dic.Key, (str, dic) => dic.Value).ToList().Adapt<List<WishlogSummaryPage_UpItem>>();
            stats.UpItems.AddRange(group.FirstOrDefault()!.Rank4UpItems.Join(dic_characters, str => str, dic => dic.Key, (str, dic) => dic.Value).Adapt<IEnumerable<WishlogSummaryPage_UpItem>>());
            var currentEventItems = wishlogs.Where(x => x.QueryType == WishType.CharacterEvent && stats.StartTime <= x.Time && x.Time <= stats.EndTime).OrderByDescending(x => x.Id).ToList();
            stats.TotalCount = currentEventItems.Count;
            stats.Rank3Count = currentEventItems.Count(x => x.RankType == 3);
            stats.Rank4Count = currentEventItems.Count(x => x.RankType == 4);
            stats.Rank5Count = currentEventItems.Count(x => x.RankType == 5);
            Parallel.ForEach(stats.UpItems, x => x.Count = currentEventItems.Count(y => y.Name == x.Name));
            stats.Rank5Items = queryTypeStats?.FirstOrDefault(x => x.QueryType == WishType.CharacterEvent)?.Rank5Items
                                              .Where(x => x.Time >= stats.StartTime && x.Time <= stats.EndTime)
                                              .ToList() ?? new();
        }
        var characterEventStats = character_eventStats.OrderByDescending(x => x.StartTime).ToList();



        var weapon_groups = eventInfos.Where(x => x.QueryType == WishType.WeaponEvent).GroupBy(x => x.StartTime).ToList();
        var weapon_eventStats = new List<WishlogSummaryPage_EventStats>();
        foreach (var group in weapon_groups)
        {
            var stats = group.FirstOrDefault()?.Adapt<WishlogSummaryPage_EventStats>()!;
            weapon_eventStats.Add(stats);
            stats.Name = string.Join("\n", group.Select(x => x.Name));
            stats.UpItems = group.SelectMany(x => x.Rank5UpItems).Join(dic_weapons, str => str, dic => dic.Key, (str, dic) => dic.Value).ToList().Adapt<List<WishlogSummaryPage_UpItem>>();
            stats.UpItems.AddRange(group.FirstOrDefault()!.Rank4UpItems.Join(dic_weapons, str => str, dic => dic.Key, (str, dic) => dic.Value).Adapt<IEnumerable<WishlogSummaryPage_UpItem>>());
            var currentEventItems = wishlogs.Where(x => x.QueryType == WishType.WeaponEvent && stats.StartTime <= x.Time && x.Time <= stats.EndTime).OrderByDescending(x => x.Id).ToList();
            stats.TotalCount = currentEventItems.Count;
            stats.Rank3Count = currentEventItems.Count(x => x.RankType == 3);
            stats.Rank4Count = currentEventItems.Count(x => x.RankType == 4);
            stats.Rank5Count = currentEventItems.Count(x => x.RankType == 5);
            Parallel.ForEach(stats.UpItems, x => x.Count = currentEventItems.Count(y => y.Name == x.Name));
            stats.Rank5Items = queryTypeStats?.FirstOrDefault(x => x.QueryType == WishType.WeaponEvent)?.Rank5Items
                                              .Where(x => x.Time >= stats.StartTime && x.Time <= stats.EndTime)
                                              .ToList() ?? new();
        }
        var weaponEventStats = weapon_eventStats.OrderByDescending(x => x.StartTime).ToList();


        return new LoadWishlogResult
        {
            CharacterThumbs = characterThumbs,
            QueryTypeStats = queryTypeStats,
            WeaponThumbs = weaponThumbs,
            CharacterEventStats = characterEventStats,
            WeaponEventStats = weaponEventStats,
        };
    }



    private class LoadWishlogResult
    {
        public List<WishlogSummaryPage_QueryTypeStats> QueryTypeStats { get; set; }


        public List<WishlogSummaryPage_ItemThumb> CharacterThumbs { get; set; }


        public List<WishlogSummaryPage_ItemThumb> WeaponThumbs { get; set; }


        public List<WishlogSummaryPage_EventStats> CharacterEventStats { get; set; }


        public List<WishlogSummaryPage_EventStats> WeaponEventStats { get; set; }
    }




    private async void _proxyService_GotWishlogUrl(object? sender, string e)
    {
        if (string.IsNullOrWhiteSpace(e))
        {
            return;
        }
        try
        {
            var uid = await _wishlogService.GetUidByWishlogUrl(e);
            _proxyService.StopProxy();
            await ToastProvider.SendAsync("完成", $"已获取 Uid {uid} 的祈愿记录网址");
            MainWindow.Current.DispatcherQueue.TryEnqueue(async () =>
            {
                try
                {
                    ClipboardHelper.SetText(e);
                    NotificationProvider.Success("完成", $"已复制 Uid {uid} 的祈愿记录网址到剪贴板", 5000);
                    var addCount = await _wishlogService.GetWishlogByUidAsync(uid, StateTextHandler);
                    OperationHistory.AddToDatabase("GetWishlog", uid.ToString());
                    StateText = $"新增 {addCount} 条祈愿记录";
                    SelectedUid = uid;
                    InitializePageData();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            });
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "获取新的祈愿记录网址");
            // 内部切换到 UI 线程
            NotificationProvider.Error(ex, "获取新的祈愿记录网址");
        }
    }




    #region Control Event





    private async void ComboBox_AllUids_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (e.AddedItems.FirstOrDefault() is string { Length: > 0 } UidString)
            {
                if (int.TryParse(UidString, out int uid))
                {
                    await ChangeUidAsync(uid);
                    AppSetting.SetValue(SettingKeys.LastSelectedUidInWishlogSummaryPage, uid);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
    }





    private void _TextBlock_Rank5Item_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is TextBlock textblock)
        {
            if (textblock.DataContext is WishlogSummaryPage_Rank5Item item && textblock.Tag is WishlogSummaryPage_QueryTypeStats stats)
            {
                var name = item.Name;
                var color = item.Color == "#808080" ? "#5470C6" : item.Color;
                stats.Rank5Items.ForEach(x =>
                {
                    if (x.Name == name)
                    {
                        x.Foreground = color;
                    }
                    else
                    {
                        x.Foreground = "#808080"; //gray
                    }
                });
            }
        }
    }


    private void _ScrollViewer_Rank5Item_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is ScrollViewer scroll)
        {
            if (scroll.DataContext is WishlogSummaryPage_QueryTypeStats stats)
            {
                stats.Rank5Items.ForEach(x =>
                {
                    x.Foreground = x.Color;
                });
            }
        }
    }


    private void _Button_ExpandScrollViewer_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            if (button.Tag is ScrollViewer scroll)
            {
                if (scroll.Tag is Grid grid)
                {
                    var sb = new Storyboard();
                    var ani = new DoubleAnimation { Duration = new Duration(TimeSpan.FromMilliseconds(167)), EnableDependentAnimation = true };
                    Storyboard.SetTarget(ani, grid);
                    Storyboard.SetTargetProperty(ani, "Height");
                    if (grid.Tag as string == "HasExpand")
                    {
                        ani.From = grid.ActualHeight;
                        ani.To = 240;
                        grid.Tag = "";
                        button.Content = "\xE70D"; //ChevronDown
                    }
                    else
                    {
                        ani.From = grid.ActualHeight;
                        if (scroll.ExtentHeight <= 152)
                        {
                            ani.To = 240;
                        }
                        else
                        {
                            ani.To = 68 + scroll.ExtentHeight;
                        }
                        grid.Tag = "HasExpand";
                        grid.MaxHeight = double.PositiveInfinity;
                        button.Content = "\xE70E"; //ChevronUp
                    }
                    sb.Children.Add(ani);
                    sb.Begin();
                }

            }
        }
    }



    #endregion





    #region Get Wishlog



    /// <summary>
    /// 从缓存文件获取祈愿记录
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task GetWishlogFromCacheFileAsync(string server)
    {
        try
        {
            var url = server switch
            {
                "0" or "1" => WishlogClient.GetWishlogUrlFromCacheFile(GameAccountService.GetGameExePath(server == "1" ? 1 : 0)),
                "2" => WishlogService.FindWishlogUrlFromCloudServer(),
                _ => null,
            };
            if (url is null)
            {
                NotificationProvider.Warning("没有找到祈愿记录网址，请在游戏中打开历史记录页面后再重试。");
                return;
            }
            try
            {
                StateText = $"检查祈愿记录网址的有效性";
                var uid = await _wishlogService.GetUidByWishlogUrl(url);
                var addCount = await _wishlogService.GetWishlogByUidAsync(uid, StateTextHandler);
                OperationHistory.AddToDatabase("GetWishlog", uid.ToString());
                StateText = $"新增 {addCount} 条祈愿记录";
                SelectedUid = uid;
                InitializePageData();
            }
            catch (Exception ex) when (ex is HoyolabException or XunkongException)
            {
                StateText = ex.Message;
                if (ex is HoyolabException { ReturnCode: -101 })
                {
                    WishlogService.DeleteCacheFile(int.Parse(server));
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "获取祈愿记录");
            NotificationProvider.Error(ex, "获取祈愿记录");
        }
    }



    /// <summary>
    /// 启动代理
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task StartProxyAsync()
    {
        try
        {
            await ShowProxyDialogAsync();
        }
        catch (Exception ex)
        {
            StateText = "";
            Logger.Error(ex, "启动代理");
            NotificationProvider.Error(ex, "启动代理");
        }
    }



    /// <summary>
    /// 显示代理提示
    /// </summary>
    /// <returns></returns>
    private async Task ShowProxyDialogAsync()
    {
        StateText = "";
        var stackPanel = new StackPanel { Spacing = 8 };
        stackPanel.Children.Add(new TextBlock { Text = "您需要重新获取祈愿记录网址，点击下方启动代理按键后，在原神游戏中重新打开祈愿记录页面，获取到网址后再次点击获取记录。", TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap });
        stackPanel.Children.Add(new TextBlock { Text = "获取到网址后应用会自动关闭代理，若关闭应用后出现无法连接网络的情况，请在「设置/网络和 Internet/代理/使用代理服务器」中手动关闭代理。", TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap });
        stackPanel.Children.Add(new TextBlock { Text = "首次启动代理时必须安装证书，否则无法获取网址，此证书为软件自动生成。", TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap });
        var dialog = new ContentDialog
        {
            Title = "启动代理服务器",
            Content = stackPanel,
            PrimaryButtonText = "启动代理",
            SecondaryButtonText = "关闭",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = MainWindow.Current.XamlRoot,
            RequestedTheme = MainWindow.Current.ActualTheme,
        };
        if (await dialog.ShowWithZeroMarginAsync() == ContentDialogResult.Primary)
        {
            _proxyService.StartProxy();
            if (_proxyService.CheckSystemProxy())
            {
                NotificationProvider.Success("已启动代理", "代理服务的端口为 localhost:10086，在原神游戏中重新打开祈愿记录页面。", 10000);
            }
            else
            {
                NotificationProvider.ShowWithButton(InfoBarSeverity.Warning,
                                                    "有问题",
                                                    "代理服务的端口为 localhost:10086，但是系统代理设置出错。",
                                                    "打开代理设置",
                                                    async () => await Launcher.LaunchUriAsync(new("ms-settings:network-proxy")),
                                                    null,
                                                    10000);
            }
        }
    }



    /// <summary>
    /// 关闭代理
    /// </summary>
    [RelayCommand]
    private void CloseProxy()
    {
        try
        {
            if (_proxyService.StopProxy())
            {
                NotificationProvider.Success("代理已关闭");
            }
            else
            {
                NotificationProvider.Success("代理早已关闭");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "关闭代理");
            NotificationProvider.Error(ex, "关闭代理");
        }
    }




    /// <summary>
    /// 使用已选择uid保存在本地数据库的祈愿记录网址获取新的祈愿记录
    /// </summary>
    /// <param name="getAll"></param>
    /// <returns></returns>
    [RelayCommand]
    private async Task GetWishlogFromSavedWishlogUrlAsync(bool getAll)
    {
        if (SelectedUid == 0)
        {
            return;
        }
        var uid = SelectedUid;
        await Task.Delay(100);
        try
        {
            StateText = "验证祈愿记录网址的有效性";
            if (await _wishlogService.CheckWishlogUrlTimeoutAsync(uid))
            {
                var addCount = await _wishlogService.GetWishlogByUidAsync(uid, StateTextHandler, getAll);
                StateText = $"新增 {addCount} 条祈愿记录";
            }
            else
            {
                StateText = "网址已过期";
                return;
            }
            OperationHistory.AddToDatabase("GetWishlog", uid.ToString());
            SelectedUid = uid;
            InitializePageData();
        }
        catch (Exception ex) when (ex is HoyolabException or XunkongException)
        {
            StateText = ex.Message;
        }
        catch (Exception ex)
        {
            StateText = "";
            NotificationProvider.Error(ex);
            Logger.Error(ex, "从保存的网址获取新的祈愿记录");
        }
    }



    /// <summary>
    /// 从输入的祈愿记录网址获取信息祈愿记录
    /// </summary>
    /// <param name="wishlogUrl"></param>
    /// <returns></returns>
    [RelayCommand]
    private async Task GetWishlogFromInputWishlogUrlAsync()
    {
        var textBox = new TextBox();
        var dialog = new ContentDialog
        {
            Title = "输入祈愿记录网址",
            Content = textBox,
            PrimaryButtonText = "确认",
            SecondaryButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = MainWindow.Current.XamlRoot,
            RequestedTheme = MainWindow.Current.ActualTheme,
        };
        if (await dialog.ShowWithZeroMarginAsync() == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(textBox.Text))
        {
            await Task.Delay(100);
            var wishlogUrl = textBox.Text;
            try
            {
                StateText = "验证祈愿记录网址的有效性";
                var uid = await _wishlogService.GetUidByWishlogUrl(wishlogUrl);
                var addCount = await _wishlogService.GetWishlogByUidAsync(uid, StateTextHandler);
                OperationHistory.AddToDatabase("GetWishlog", uid.ToString());
                StateText = $"新增 {addCount} 条祈愿记录";
                SelectedUid = uid;
                InitializePageData();
            }
            catch (Exception ex) when (ex is HoyolabException or XunkongException)
            {
                StateText = ex.Message;
            }
            catch (Exception ex)
            {
                StateText = "";
                NotificationProvider.Error(ex);
                Logger.Error(ex, "从输入的网址获取新的祈愿记录");
            }
        }
    }



    #endregion






    #region Backup Wishlog


    /// <summary>
    /// 祈愿记录备份用户须知
    /// </summary>
    /// <returns></returns>
    private async Task<bool> RequestWishlogBackupAgreementAsync()
    {
        if (AppSetting.TryGetValue<bool>(SettingKeys.WishlogBackupAgreement, out var agree))
        {
            if (agree)
            {
                return true;
            }
        }
        var dialog = new ContentDialog
        {
            Title = "祈愿记录备份用户须知",
            Content = """
            我们无法保证祈愿记录备份的可靠性，您仍需自行保管好本地数据。
            此功能需要上传祈愿记录网址至服务端以验证账号信息，我们不会将您的数据用于别处。
            """,
            PrimaryButtonText = "已了解",
            SecondaryButtonText = "不接受",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = MainWindow.Current.XamlRoot,
            RequestedTheme = MainWindow.Current.ActualTheme,
        };
        if (await dialog.ShowWithZeroMarginAsync() == ContentDialogResult.Primary)
        {
            AppSetting.SetValue(SettingKeys.WishlogBackupAgreement, true);
            return true;
        }
        else
        {
            return false;
        }
    }


    /// <summary>
    /// 查询指定uid备份在寻空服务器上的祈愿记录
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task QueryWishlogInCloudAsync()
    {
        if (SelectedUid == 0)
        {
            return;
        }
        if (!await RequestWishlogBackupAgreementAsync())
        {
            return;
        }
        var uid = SelectedUid;
        StateText = "";
        await Task.Delay(100);
        try
        {
            await _xunkongApiService.GetWishlogBackupLastItemAsync(uid, StateTextHandler);
        }
        catch (XunkongException ex)
        {
            StateText = ex.Message;
        }
        catch (Exception ex)
        {
            StateText = "";
            NotificationProvider.Error(ex);
            Logger.Error(ex, "查询祈愿记录备份");
        }
    }


    /// <summary>
    /// 上传指定uid的祈愿记录至寻空服务器
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task UploadWishlogToCloudAsync(bool putAll)
    {
        if (SelectedUid == 0)
        {
            return;
        }
        if (!await RequestWishlogBackupAgreementAsync())
        {
            return;
        }
        var uid = SelectedUid;
        StateText = "";
        await Task.Delay(100);
        try
        {
            await _xunkongApiService.PutWishlogListAsync(uid, StateTextHandler, putAll);
        }
        catch (XunkongException ex)
        {
            StateText = ex.Message;
        }
        catch (Exception ex)
        {
            StateText = "";
            NotificationProvider.Error(ex);
            Logger.Error(ex, "上传祈愿记录备份");
        }
    }


    /// <summary>
    /// 从寻空服务器下载指定uid的祈愿记录
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task DownloadWishlogFromCloudCompletelyAsync()
    {
        if (SelectedUid == 0)
        {
            return;
        }
        if (!await RequestWishlogBackupAgreementAsync())
        {
            return;
        }
        var uid = SelectedUid;
        StateText = "";
        await Task.Delay(100);
        try
        {
            await _xunkongApiService.GetWishlogBackupListAsync(uid, StateTextHandler, true);
        }
        catch (XunkongException ex)
        {
            StateText = ex.Message;
        }
        catch (Exception ex)
        {
            StateText = "";
            NotificationProvider.Error(ex);
            Logger.Error(ex, "下载祈愿记录备份");
        }
    }



    /// <summary>
    /// 删除指定uid备份在寻空服务器的祈愿记录，并将记录备份在本地
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task DeleteWishlogInCloudCompletelyAsync()
    {
        if (SelectedUid == 0)
        {
            return;
        }
        if (!await RequestWishlogBackupAgreementAsync())
        {
            return;
        }
        var uid = SelectedUid;
        StateText = "";
        try
        {
            var result = await _xunkongApiService.GetWishlogBackupLastItemAsync(uid, StateTextHandler);
            if (result.CurrentCount == 0)
            {
                StateText = "云端没有记录";
                return;
            }
            if (result.CurrentCount > WishlogService.GetWishlogCount(uid))
            {
                var dialog = new ContentDialog
                {
                    Title = "警告",
                    Content = $"Uid {uid} 的本地祈愿记录数量少于云端，删除云端记录可能会造成数据丢失，是否继续？",
                    PrimaryButtonText = "继续删除",
                    CloseButtonText = "取消",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = MainWindow.Current.XamlRoot,
                    RequestedTheme = MainWindow.Current.ActualTheme,
                };
                if (await dialog.ShowWithZeroMarginAsync() != ContentDialogResult.Primary)
                {
                    return;
                }
            }
            var file = await _xunkongApiService.GetWishlogAndBackupToLoalAsync(uid, StateTextHandler);
            Action action = () => Process.Start(new ProcessStartInfo { FileName = Path.GetDirectoryName(file), UseShellExecute = true, });
            NotificationProvider.ShowWithButton(InfoBarSeverity.Success, null, "云端祈愿记录已备份到本地", "打开文件夹", action, null, 3000);
            await _xunkongApiService.DeleteWishlogBackupAsync(uid, StateTextHandler);
        }
        catch (XunkongException ex)
        {
            StateText = ex.Message;
        }
        catch (Exception ex)
        {
            StateText = "";
            NotificationProvider.Error(ex);
            Logger.Error(ex, "删除祈愿记录备份");
        }
    }



    #endregion




    /// <summary>
    /// 导航到祈愿记录管理页面
    /// </summary>
    [RelayCommand]
    private void NavigateToWishlogManagePage()
    {
        try
        {
            MainPage.Current.Navigate(typeof(WishlogManagePage), SelectedUid);
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex);
            Logger.Error(ex, "导航到祈愿记录管理页面");
        }
    }



    /// <summary>
    /// 导航到祈愿活动历史记录页面
    /// </summary>
    [RelayCommand]
    private void NavigateToWishEventHistoryPage()
    {
        try
        {
            MainPage.Current.Navigate(typeof(WishEventHistoryPage));
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex);
            Logger.Error(ex, "导航到祈愿活动历史记录页面");
        }
    }




    private void Grid_ItemThumb_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (sender is Grid grid && grid.Tag is WishlogSummaryPage_ItemThumb item)
        {
            ItemShowList = wishlogList?.Where(x => x.Name == item.Name).OrderBy(x => x.Id).ToList();
            FlyoutBase.ShowAttachedFlyout(Grid_Content);
        }
    }



}
