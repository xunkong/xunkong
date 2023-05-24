using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using ProtoBuf;
using SingleFileExtractor.Core;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Json;
using System.Numerics;
using System.Reflection;
using System.Text.Json.Nodes;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI;
using Xunkong.Desktop.Controls;
using Xunkong.GenshinData.Achievement;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class AchievementPage : Page
{


    // 成就组背景图片，分选中和非选中两种
    private BitmapImage mapTitleBgNonSelected = new(new("ms-appx:///Assets/Images/UI_Img_MapTitle_Bg_NonSelected.png"));
    private BitmapImage mapTitleBgSelected = new(new("ms-appx:///Assets/Images/UI_Img_MapTitle_Bg_Selected.png"));


    public AchievementPage()
    {
        this.InitializeComponent();
        NavigationCacheMode = AppSetting.GetValue<bool>(SettingKeys.EnableNavigationCache) ? NavigationCacheMode.Enabled : NavigationCacheMode.Disabled;
        WeakReferenceMessenger.Default.Register<ProtocolMessage>(this, (_, e) => HandleProtocolMessage(e));
        Loaded += AchievementPage_Loaded;
        Unloaded += AchievementPage_Unloaded;
    }




    /// <summary>
    /// 协议消息处理方法
    /// </summary>
    /// <param name="e"></param>
    private void HandleProtocolMessage(ProtocolMessage e)
    {
        if (e.Message == ProtocolMessage.ChangeSelectedUidInAchievementPage)
        {
            if (int.TryParse(e.Data.GetValueOrDefault("uid"), out int uid))
            {
                if (uid > 0)
                {
                    var lastSelctedUid = SelectedUid;
                    SelectedUid = uid;
                    if (!Uids.Contains(uid))
                    {
                        Uids.Add(uid);
                    }
                    c_ComboBox_Uids.SelectedValue = SelectedUid;
                    // 手动刷新页面
                    if (lastSelctedUid == SelectedUid)
                    {
                        OnSelectedUidChanged(selectedUid);
                    }
                }
            }
        }
    }


    [ObservableProperty]
    private ObservableCollection<int> uids;

    [ObservableProperty]
    private int selectedUid;

    /// <summary>
    /// 显示的成就，所属成就组 或 搜索
    /// </summary>
    [ObservableProperty]
    private List<AchievementPageModel_Item> achievements;

    /// <summary>
    /// 所有成就组
    /// </summary>
    [ObservableProperty]
    private List<AchievementPageModel_Goal> achievementGoals;

    /// <summary>
    /// 选中的成就组
    /// </summary>
    [ObservableProperty]
    private AchievementPageModel_Goal selectedGoal;


    private List<AchievementPageModel_Item> original_items;

    private Dictionary<int, AchievementPageModel_Item> original_items_dic;

    /// <summary>
    /// 全部成就数
    /// </summary>
    [ObservableProperty]
    private int totalCount;

    /// <summary>
    /// 已完成的成就数
    /// </summary>
    [ObservableProperty]
    private int finishedCount;

    /// <summary>
    /// 全部原石奖励
    /// </summary>
    [ObservableProperty]
    private int totalRewardCount;

    /// <summary>
    /// 已达成的原石奖励
    /// </summary>
    [ObservableProperty]
    private int gotRewardCount;


    private bool preCached;


    private async void AchievementPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (Uids is null)
        {
            await Task.Delay(60);
            InitializeData();
        }
    }


    private void AchievementPage_Unloaded(object sender, RoutedEventArgs e)
    {
        WeakReferenceMessenger.Default.Unregister<ProtocolMessage>(this);
    }


    [RelayCommand]
    private void InitializeData()
    {
        try
        {
            Uids = new(LoadAchievementUids());
            var lastSelectedUid = SelectedUid;
            if (AppSetting.TryGetValue(SettingKeys.LastSelectedUidInAchievementPage, out int uid) && Uids.Contains(uid))
            {
                SelectedUid = uid;
            }
            else
            {
                SelectedUid = Uids.FirstOrDefault();
            }
            if (selectedUid > 0)
            {
                c_ComboBox_Uids.SelectedValue = SelectedUid;
            }
            // 手动刷新页面
            if (lastSelectedUid == SelectedUid)
            {

                OnSelectedUidChanged(selectedUid);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "初始化成就页面");
            NotificationProvider.Error(ex, "初始化成就页面");
        }
    }


    /// <summary>
    /// 切换uid
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void c_ComboBox_Uids_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var uid = e.AddedItems.FirstOrDefault();
        if (uid is int i and > 0)
        {
            SelectedUid = i;
            AppSetting.SetValue(SettingKeys.LastSelectedUidInAchievementPage, SelectedUid);
        }
    }


    /// <summary>
    /// Uid 变更时加载新的数据
    /// </summary>
    /// <param name="value"></param>
    partial void OnSelectedUidChanged(int value)
    {
        try
        {
            FinishedCount = 0;
            GotRewardCount = 0;
            TotalRewardCount = 0;
            editable = false;

            var goals = XunkongApiService.GetGenshinData<AchievementGoal>().Adapt<List<AchievementPageModel_Goal>>();
            var items = XunkongApiService.GetGenshinData<AchievementItem>().Adapt<List<AchievementPageModel_Item>>();
            var items_dic = items.ToDictionary(x => x.Id);

            if (!preCached)
            {
                // 预下载图片
                goals.Select(x => XunkongCache.Instance.PreCacheAsync(new Uri(x.IconPath))).ToList();
                preCached = true;
            }

            var data_dic = LoadAchievementDataItems(value).ToDictionary(x => x.Id);

            foreach (var item in items)
            {
                // 成就数据和用户数据结合
                if (data_dic.TryGetValue(item.Id, out var data))
                {
                    item.Uid = data.Uid;
                    item.Current = data.Current;
                    item.Status = data.Status;
                    item.FinishedTime = data.FinishedTime;
                    item.Comment = data.Comment;
                    item.LastUpdateTime = data.LastUpdateTime;
                }

                // 关联相同名称的成就
                if (item.PreStageAchievementId != 0)
                {
                    var stack = new Stack<AchievementPageModel_Item>();
                    stack.Push(item);
                    var nextId = item.Id;
                    var lastId = item.PreStageAchievementId;
                    while (items_dic.TryGetValue(lastId, out var lastItem))
                    {
                        stack.Push(lastItem);
                        lastItem.NextAchievementId = nextId;
                        lastId = lastItem.PreStageAchievementId;
                        nextId = lastItem.Id;
                    }
                    int maxStar = 0;
                    int currentStar = 0;
                    while (stack.TryPop(out var thisItem))
                    {
                        maxStar++;
                        if (thisItem.IsFinish)
                        {
                            currentStar++;
                        }
                        thisItem.CurrentStar = currentStar;
                        if (maxStar > thisItem.MaxStar)
                        {
                            thisItem.MaxStar = maxStar;
                        }
                    }
                }
            }

            // 分组
            foreach (var goal in goals)
            {
                var goalItems = items.Where(x => x.GoalId == goal.Id).OrderBy(x => x.IsFinish).ThenBy(x => x.OrderId).ToList();
                goal.Items = goalItems;
                goal.Total = goalItems.Count;
                goal.Current = goalItems.Count(x => x.IsFinish);
                goal.GotRewardCount = goalItems.Where(x => x.IsFinish).Sum(x => x.RewardCount);
                goal.TotalRewardCount = goalItems.Sum(x => x.RewardCount);
                GotRewardCount += goal.GotRewardCount;
                TotalRewardCount += goal.TotalRewardCount;
                if (goal.IsFinish)
                {
                    goal.FinishedTime = goalItems.Max(x => x.FinishedTime)!;
                }
            }

            c_Grid_GoalReward.Visibility = Visibility.Visible;
            var lastSelectedGoalId = SelectedGoal?.Id;
            AchievementGoals = goals.OrderBy(x => x.OrderId).ToList();
            SelectedGoal = AchievementGoals.FirstOrDefault(x => x.Id == lastSelectedGoalId) ?? AchievementGoals.FirstOrDefault()!;
            Achievements = SelectedGoal?.Items!;

            original_items = items;
            original_items_dic = items_dic;
            TotalCount = items.Count;
            FinishedCount = items.Count(x => x.IsFinish);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "加载成就数据");
            NotificationProvider.Error(ex, "加载成就数据");
        }
    }


    /// <summary>
    /// 切换成就组
    /// </summary>
    /// <param name="value"></param>
    partial void OnSelectedGoalChanged(AchievementPageModel_Goal value)
    {
        if (value != null)
        {
            Achievements = value.Items;
            c_Grid_GoalReward.Visibility = Visibility.Visible;
            var container = c_ListView_AchievementGoal.ContainerFromItem(value);
            if (container is ListViewItem item)
            {
                if (item.ContentTemplateRoot is Border border)
                {
                    SelectedAchievementGoalBorder = border;
                }
            }
        }
    }



    private List<int> LoadAchievementUids()
    {
        using var dapper = DatabaseProvider.CreateConnection();
        return dapper.Query<int>("SELECT DISTINCT Uid FROM AchievementData;").ToList();
    }



    private List<AchievementData> LoadAchievementDataItems(int uid)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        return dapper.Query<AchievementData>("SELECT * FROM AchievementData WHERE Uid=@Uid;", new { Uid = uid }).ToList();
    }


    /// <summary>
    /// 搜索
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void c_AutoSuggetBox_Search_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(sender.Text))
            {
                c_Grid_GoalReward.Visibility = Visibility.Visible;
                Achievements = SelectedGoal.Items;
            }
            else
            {
                var splitText = sender.Text.Split(" ");
                var result = original_items;
                foreach (var text in splitText)
                {
                    if (int.TryParse(text, out int id))
                    {
                        result = result.Where(x => x.Id == id).ToList();
                    }
                    else
                    {
                        result = result.Where(x => x.Title.Contains(text) || x.Description.Contains(text) || (x.Version?.Contains(text) ?? false)).ToList();
                    }
                }
                c_Grid_GoalReward.Visibility = Visibility.Collapsed;
                Achievements = result.OrderBy(x => x.IsFinish).ThenBy(x => x.OrderId).ToList();
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "搜索成就");
        }
    }


    /// <summary>
    /// 启动成就获取工具
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task StartYaeAchievementAsync()
    {
        try
        {
            var dialog = new ContentDialog
            {
                Title = new TextBlock { Text = "严重警告", FontSize = 20, Foreground = Application.Current.Resources["SystemFillColorCriticalBrush"] as Brush },
                Content = new YaeAchievementAlert(),
                PrimaryButtonText = "接受风险",
                SecondaryButtonText = "关闭",
                DefaultButton = ContentDialogButton.Secondary,
                XamlRoot = MainWindow.Current.XamlRoot,
                RequestedTheme = MainWindow.Current.ActualTheme,
            };
            if (await dialog.ShowWithZeroMarginAsync() == ContentDialogResult.Primary)
            {
                if (GameAccountService.IsGameRunning(0) || GameAccountService.IsGameRunning(1))
                {
                    NotificationProvider.Warning("已有游戏进程在运行");
                    return;
                }
                await CheckYaeAchievementUpdateAsync();
                var yaePath = Path.Combine(XunkongEnvironment.UserDataPath, @"Tool\YaeAchievement.exe");
                if (!File.Exists(yaePath))
                {
                    return;
                }
                var info = new ProcessStartInfo
                {
                    FileName = yaePath,
                    UseShellExecute = true,
                    Verb = "runas",
                };
                Process.Start(info);
                OperationHistory.AddToDatabase("StartYaeAchievement");
                Logger.TrackEvent("StartYaeAchievement");
            }
        }
        catch (Exception ex)
        {
            if (ex is Win32Exception ex32)
            {
                if (ex32.NativeErrorCode == 1223)
                {
                    // 操作已取消
                    return;
                }
            }
            NotificationProvider.Error(ex, "启动 YaeAchievement");
            Logger.Error(ex, "启动 YaeAchievement");
        }
    }





    private async Task CheckYaeAchievementUpdateAsync()
    {
        const string HOST = "https://cn-cd-1259389942.file.myqcloud.com";
        try
        {
            var path = Path.Combine(XunkongEnvironment.UserDataPath, "Tool\\YaeAchievement.exe");
            var client = new HttpClient();
            var versionBytesTask = client.GetByteArrayAsync($"{HOST}/schicksal/version");
            uint versionCode = 0;
            if (File.Exists(path))
            {
                var entry = new ExecutableReader().ReadManifest(path).Files.FirstOrDefault(x => x.RelativePath.Contains("YaeAchievement.dll"));
                if (entry != null)
                {
                    using var fs = File.OpenRead(path);
                    var bytes = new byte[entry.Size];
                    fs.Position = entry.Offset;
                    if (entry.IsCompressed)
                    {
                        using var deflate = new DeflateStream(fs, CompressionMode.Decompress);
                        deflate.Read(bytes);
                    }
                    else
                    {
                        fs.Read(bytes);
                    }
                    var assembly = Assembly.Load(bytes);
                    var value = assembly.GetType("YaeAchievement.GlobalVars")?.GetField("AppVersionCode")?.GetRawConstantValue();
                    if (value is uint v)
                    {
                        versionCode = v;
                    }
                }
            }
            var versionBytes = await versionBytesTask;
            var version = Serializer.Deserialize<YaeVersion>(new ReadOnlySpan<byte>(versionBytes));
            if (version.VersionCode != versionCode)
            {
                NotificationProvider.Information("正在下载 YaeAchievement");
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                var bytes = await client.GetByteArrayAsync($"{HOST}/{version.PackageLink}");
                await File.WriteAllBytesAsync(path, bytes);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex, "Update YaeAchievement");
        }
    }




    [ProtoContract]
    private class YaeVersion
    {
        [ProtoMember(1)]
        public uint VersionCode { get; set; }

        [ProtoMember(2)]
        public string VersionName { get; set; }

        [ProtoMember(3)]
        public string Description { get; set; }

        [ProtoMember(4)]
        public string PackageLink { get; set; }

        [ProtoMember(5)]
        public bool ForceUpdate { get; set; }

        [ProtoMember(6)]
        public bool EnableLibDownload { get; set; }

        [ProtoMember(7)]
        public bool EnableAutoDownload { get; set; }

    }




    #region Edit


    private bool editable;

    /// <summary>
    /// 修改编辑状态
    /// </summary>
    [RelayCommand]
    private async void ChangeEditable()
    {
        if (SelectedUid == 0)
        {
            var uid = await OpenAddUidDialogAsync();
            if (uid == 0)
            {
                return;
            }
            else
            {
                SelectedUid = uid;
            }
        }
        editable = !editable;
        if (original_items?.Any() ?? false)
        {
            foreach (var item in original_items)
            {
                item.Editable = editable;
            }
        }
    }


    /// <summary>
    /// 添加Uid的输入窗口
    /// </summary>
    /// <returns></returns>
    private async Task<int> OpenAddUidDialogAsync()
    {
        var text = new TextBox();
        var dialog = new ContentDialog
        {
            Title = "请输入 Uid",
            Content = text,
            PrimaryButtonText = "确认",
            SecondaryButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = MainWindow.Current.XamlRoot,
            RequestedTheme = MainWindow.Current.ActualTheme,
        };
        if (await dialog.ShowWithZeroMarginAsync() == ContentDialogResult.Primary)
        {
            if (int.TryParse(text.Text, out int uid))
            {
                return uid;
            }
            else
            {
                return 0;
            }
        }
        else
        {
            return 0;
        }
    }


    /// <summary>
    /// 点击达成按键
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void c_CheckBox_AchievementItemFinished_Click(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox box)
        {
            if (box.Tag is AchievementPageModel_Item item)
            {
                var newFinish = box.IsChecked ?? false;
                item.Current = 0;
                if (item.IsFinish == newFinish)
                {
                    return;
                }
                item.Status = newFinish ? 3 : 1;
                if (newFinish)
                {
                    OnAchievementFinisheChanged(item, true);
                }
                else
                {
                    OnAchievementFinisheChanged(item, false);
                }
                SaveAchievementItemStateChanged(item);
            }
        }
        if (sender is Image image)
        {
            if (image.Tag is AchievementPageModel_Item item)
            {
                var newFinish = !item.IsFinish;
                item.Current = 0;
                item.Status = newFinish ? 3 : 1;
                if (newFinish)
                {
                    OnAchievementFinisheChanged(item, true);
                }
                else
                {
                    OnAchievementFinisheChanged(item, false);
                }
                SaveAchievementItemStateChanged(item);
            }
        }
    }


    /// <summary>
    /// 修改当前值
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void c_NumberBox_AchievementItemCurrent_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is NumberBox box)
        {
            if (box.Tag is AchievementPageModel_Item thisItem)
            {
                if (int.TryParse(box.Text, out int current))
                {
                    if (current == thisItem.Current)
                    {
                        return;
                    }


                    // 找到所有相关成就
                    var list = new List<AchievementPageModel_Item>();
                    list.Add(thisItem);
                    int preId = thisItem.PreStageAchievementId;
                    while (original_items_dic?.TryGetValue(preId, out var lastItem) ?? false)
                    {
                        list.Add(lastItem);
                        preId = lastItem.PreStageAchievementId;
                    }
                    int nextId = thisItem.NextAchievementId;
                    while (original_items_dic?.TryGetValue(nextId, out var nextItem) ?? false)
                    {
                        list.Add(nextItem);
                        nextId = nextItem.NextAchievementId;
                    }

                    var goal = AchievementGoals.FirstOrDefault(x => x.Id == thisItem.GoalId);

                    // 修改属性
                    list = list.OrderBy(x => x.Id).ToList();
                    int currentStar = 0;
                    foreach (var item in list)
                    {
                        var oldFinish = item.IsFinish;
                        item.Status = 1;
                        item.Current = current;
                        if (oldFinish != item.IsFinish)
                        {
                            if (item.IsFinish)
                            {
                                currentStar++;
                                item.CurrentStar = currentStar;
                                OnAchievementFinisheChanged(item, true);
                            }
                            else
                            {
                                OnAchievementFinisheChanged(item, false);
                            }
                        }
                        item.Status = item.IsFinish ? 3 : 1;
                        SaveAchievementItemStateChanged(item);
                    }

                }
            }
        }
    }

    /// <summary>
    /// 修改完成时间
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void c_TextBox_AchievementItemFinishedTime_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox box)
        {
            if (box.Tag is AchievementPageModel_Item item)
            {
                if (DateTimeOffset.TryParse(box.Text, out var time))
                {
                    if (time == item.FinishedTime)
                    {
                        return;
                    }
                    item.FinishedTime = time;
                    SaveAchievementItemStateChanged(item);
                }
            }
        }
    }


    /// <summary>
    /// 修改备注
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void c_TextBox_AchievementComment_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox box)
        {
            if (box.Tag is AchievementPageModel_Item item)
            {
                if (string.IsNullOrWhiteSpace(item.Comment) && string.IsNullOrWhiteSpace(box.Text))
                {
                    return;
                }
                if (item.Comment?.Trim() == box.Text.Trim())
                {
                    return;
                }
                item.Comment = box.Text;
                SaveAchievementItemStateChanged(item);
            }
        }
    }


    /// <summary>
    /// 保存变更到数据库
    /// </summary>
    /// <param name="item"></param>
    private void SaveAchievementItemStateChanged(AchievementPageModel_Item item)
    {
        try
        {
            if (item.Uid == 0)
            {
                item.Uid = SelectedUid;
            }
            item.LastUpdateTime = DateTimeOffset.Now;
            using var dapper = DatabaseProvider.CreateConnection();
            dapper.Execute("INSERT OR REPLACE INTO AchievementData (Uid, Id, Current, Status, FinishedTime, Comment, LastUpdateTime) VALUES (@Uid, @Id, @Current, @Status, @FinishedTime, @Comment, @LastUpdateTime);", item);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "保存成就更改");
        }
    }



    private int randomId;


    /// <summary>
    /// 成就状态变更
    /// </summary>
    /// <param name="item"></param>
    private async void OnAchievementFinisheChanged(AchievementPageModel_Item item, bool isFinished)
    {
        try
        {
            if (item != null)
            {
                if (AchievementGoals?.FirstOrDefault(x => x.Id == item.GoalId) is AchievementPageModel_Goal goal)
                {
                    if (isFinished)
                    {
                        item.Status = 3;
                        item.FinishedTime = DateTimeOffset.Now;
                        FinishedCount++;
                        SelectedGoal.Current++;
                        GotRewardCount += item.RewardCount;
                        goal.GotRewardCount += item.RewardCount;
                        if (goal.Current == goal.Total)
                        {
                            goal.FinishedTime = DateTimeOffset.Now;
                            MainWindow.Current.SetFullWindowContent(new AchievementGoalFinishedPush(goal));
                        }
                        else
                        {
                            var id = Random.Shared.Next();
                            randomId = id;
                            c_Grid_Push.Opacity = 1;
                            c_TextBlock_Push.Text = item.Title;
                            c_Image_Push.Source = goal.IconPath;
                            await Task.Delay(2000);
                            if (randomId == id)
                            {
                                c_Grid_Push.Opacity = 0;
                            }
                        }
                    }
                    else
                    {
                        item.Status = 1;
                        item.FinishedTime = DateTimeOffset.UnixEpoch;
                        FinishedCount--;
                        SelectedGoal.Current--;
                        GotRewardCount -= item.RewardCount;
                        goal.GotRewardCount -= item.RewardCount;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }



    #endregion




    #region Import & Export & Delete



    [ObservableProperty]
    private string? exportApp;

    [ObservableProperty]
    private string? exportAppVersion;

    [ObservableProperty]
    private string? uiafVersion;

    [ObservableProperty]
    private int importAchievementCount;


    private List<AchievementData>? importAchievementDatas;


    /// <summary>
    /// 打开导入导出面板
    /// </summary>
    [RelayCommand]
    private void OpenImportExportGrid()
    {
        c_Grid_ImportExport.Visibility = Visibility.Visible;
        ExportApp = null;
        ExportAppVersion = null;
        UiafVersion = null;
        ImportAchievementCount = 0;
        importAchievementDatas = null;
        if (SelectedUid > 0)
        {
            c_TextBox_ImportUid.Text = SelectedUid.ToString();
        }
    }


    /// <summary>
    /// 关闭导入导出面板
    /// </summary>
    [RelayCommand]
    private void CloseImportExportGrid()
    {
        c_Grid_ImportExport.Visibility = Visibility.Collapsed;
    }



    /// <summary>
    /// 选择导入文件
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task PickFileAsync()
    {
        try
        {
            var file = await FileDialogHelper.PickSingleFileAsync(MainWindow.Current.HWND, new List<(string, string)> { ("json", "*.json") });
            if (File.Exists(file))
            {
                await ParseImportFileAsync(file);
            }
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex, "选择导入文件");
            Logger.Error(ex, "选择导入文件");
        }
    }


    /// <summary>
    /// 拖入导入文件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void c_Rect_DropFile_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
        e.DragUIOverride.IsCaptionVisible = false;
        e.DragUIOverride.IsGlyphVisible = false;
    }


    /// <summary>
    /// 拖入导入文件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void c_Rect_DropFile_Drop(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var items = await e.DataView.GetStorageItemsAsync();
            if (items.Count > 0)
            {
                await ParseImportFileAsync(items.FirstOrDefault()?.Path!);
            }
        }
    }


    /// <summary>
    /// 解析导入的数据
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private async Task ParseImportFileAsync(string path)
    {
        try
        {
            var text = await File.ReadAllTextAsync(path);
            var baseNode = JsonNode.Parse(text, new JsonNodeOptions { PropertyNameCaseInsensitive = true });
            if (baseNode?["info"] is JsonNode infoNode)
            {
                ExportApp = infoNode?["export_app"]?.ToString();
                ExportAppVersion = infoNode?["export_app_version"]?.ToString();
                UiafVersion = infoNode?["uiaf_version"]?.ToString();
            }
            if (baseNode?["list"] is JsonNode listNode)
            {
                importAchievementDatas = JsonSerializer.Deserialize<List<AchievementData>>(listNode, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            if (importAchievementDatas?.Any() ?? false)
            {
                ImportAchievementCount = importAchievementDatas.Count;
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "解析导入的成就");
            NotificationProvider.Error(ex, "解析成就失败");
        }

    }


    /// <summary>
    /// 写入数据库
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task ImportAchievementData()
    {
        try
        {
            if (int.TryParse(c_TextBox_ImportUid.Text, out int uid))
            {
                if (importAchievementDatas != null)
                {
                    NotificationProvider.Information("导入中，请稍等。。。");
                    await Task.Delay(100);
                    var now = DateTimeOffset.Now;
                    foreach (var item in importAchievementDatas)
                    {
                        item.Uid = uid;
                        item.LastUpdateTime = now;
                        if (item.Status == 0)
                        {
                            item.Status = 3;
                        }
                    }
                    await Task.Run(() =>
                    {
                        using var dapper = DatabaseProvider.CreateConnection();
                        using var t = dapper.BeginTransaction();
                        dapper.Execute("""
                            INSERT INTO AchievementData (Uid, Id, Current, Status, FinishedTime, LastUpdateTime)
                            VALUES (@Uid, @Id, @Current, @Status, @FinishedTime, @LastUpdateTime)
                            ON CONFLICT DO UPDATE SET Current=@Current, Status=@Status, FinishedTime=@FinishedTime, LastUpdateTime=@LastUpdateTime;
                            """, importAchievementDatas, t);
                        t.Commit();
                    });
                    NotificationProvider.Success("导入完成", $"已为 Uid {uid} 导入成就数据 {importAchievementCount} 条");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "导入成就");
            NotificationProvider.Error(ex, "导入成就");
        }
    }


    /// <summary>
    /// 导出数据
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task ExportAchievementDataAsync()
    {
        try
        {
            if (SelectedUid == 0)
            {
                return;
            }
            var list = LoadAchievementDataItems(SelectedUid);
            if (list.Any())
            {
                object obj;
                if (c_ComboBox_ExportUiafVersion.SelectedIndex == 0)
                {
                    // UIAF v1.1
                    obj = new
                    {
                        info = new
                        {
                            export_app = "Xunkong",
                            export_app_version = XunkongEnvironment.AppVersion.ToString(),
                            uiaf_version = "v1.1",
                            export_timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                        },
                        list,
                    };
                }
                else
                {
                    // UIAF v1.0
                    obj = new
                    {
                        info = new
                        {
                            export_app = "Xunkong",
                            export_app_version = XunkongEnvironment.AppVersion.ToString(),
                            uiaf_version = "v1.0",
                            export_timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                        },
                        list = list.Where(x => x.Status > 1).Select(x => new { id = x.Id, current = x.Current, timestamp = x.FinishedTime.ToUnixTimeSeconds() }),
                    };
                }

                var text = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
                switch (c_ComboBox_ExportTarget.SelectedIndex)
                {
                    case 0:
                        // 剪贴板
                        ClipboardHelper.SetText(text);
                        NotificationProvider.Success("导出完成", $"Uid {SelectedUid} 的所有成就已复制到剪贴板");
                        break;
                    case 1:
                        // json 文件
                        var filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $@"Xunkong\Export\Achievement\achievement_{SelectedUid}_{DateTimeOffset.Now:yyyyMMdd_HHmmss}.json");
                        Directory.CreateDirectory(Path.GetDirectoryName(filename)!);
                        await File.WriteAllTextAsync(filename, text);
                        Action action = () => Process.Start(new ProcessStartInfo { FileName = Path.GetDirectoryName(filename), UseShellExecute = true });
                        NotificationProvider.ShowWithButton(InfoBarSeverity.Success, "导出完成", $"已导出 Uid {SelectedUid} 的所有成就", "打开文件夹", action);
                        break;
                    case 2:
                        // 椰羊
                        var client = ServiceProvider.GetService<HttpClient>()!;
                        var result = await client.PostAsJsonAsync("https://77.xyget.cn/v2/memo?source=Xunkong", obj);
                        var node = await result.Content.ReadFromJsonAsync<JsonNode>();
                        var key = node?["key"]?.ToString();
                        if (string.IsNullOrWhiteSpace(key))
                        {
                            NotificationProvider.Warning("无法生成椰羊的分享链接");
                        }
                        else
                        {
                            Process.Start(new ProcessStartInfo { FileName = $"https://cocogoat.work/achievement?memo={key}", UseShellExecute = true });
                        }
                        break;
                    case 3:
                        // Snap Hutao
                        ClipboardHelper.SetText(text);
                        if (!await Launcher.LaunchUriAsync(new("hutao://achievement/import"), new LauncherOptions { FallbackUri = new("https://hut.ao/") }))
                        {
                            NotificationProvider.Warning("启动 Snap Hutao 失败");
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "导出成就");
        }
    }


    /// <summary>
    /// 添加 Uid
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task AddUidAsync()
    {
        var uid = await OpenAddUidDialogAsync();
        if (uid == 0)
        {
            return;
        }
        else
        {
            if (Uids != null && !Uids.Contains(uid))
            {
                Uids.Add(uid);
            }
            c_ComboBox_Uids.SelectedValue = uid;
            SelectedUid = uid;
        }
    }


    /// <summary>
    /// 删除数据
    /// </summary>
    [RelayCommand]
    private void Delete()
    {
        try
        {
            if (SelectedUid == 0)
            {
                return;
            }
            using var dapper = DatabaseProvider.CreateConnection();
            dapper.Execute("DELETE FROM AchievementData WHERE Uid=@Uid;", new { Uid = SelectedUid });
            NotificationProvider.Success("删除完成", $"已删除 Uid {SelectedUid} 的所有成就");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "删除成就");
        }
    }




    #endregion






    #region Animation

    [ObservableProperty]
    private Grid selectedAchievementItemGrid;

    // #F5F1EB
    private SolidColorBrush ItemGridNonPressedBackgroundBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xF5, 0xF1, 0xEB));
    // FEFEFE
    private SolidColorBrush ItemGridPressedBackgroundBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xFE, 0xFE, 0xFE));
    // E0D6CB
    private SolidColorBrush ItemGridNonSelectedBorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xE0, 0xD6, 0xCB));
    // CF9F70
    private SolidColorBrush ItemGridSelectedBorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xCF, 0x9F, 0x70));

    partial void OnSelectedAchievementItemGridChanging(Grid value)
    {
        //if (selectedAchievementItemGrid != null)
        //{
        //    selectedAchievementItemGrid.Scale = Vector3.One;
        //    selectedAchievementItemGrid.BorderThickness = new Thickness(1);
        //    selectedAchievementItemGrid.BorderBrush = ItemGridNonSelectedBorderBrush;
        //}
        //if (value != null)
        //{
        //    value.BorderThickness = new Thickness(2);
        //    value.BorderBrush = ItemGridSelectedBorderBrush;
        //    value.Scale = new Vector3(1 + (16 / (float)value.ActualWidth), 1.06f, 1);
        //}
    }


    private void c_Grid_AchievementItem_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Grid grid)
        {
            grid.CenterPoint = new Vector3((float)grid.ActualWidth / 2, (float)grid.ActualHeight / 2, 1);
            grid.Scale = new Vector3(1 + (16 / (float)grid.ActualWidth), 1.06f, 1);
        }
    }

    private void c_Grid_AchievementItem_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Grid grid)
        {
            if (selectedAchievementItemGrid != grid)
            {
                grid.Scale = Vector3.One;
            }
            grid.Background = ItemGridNonPressedBackgroundBrush;
        }
    }

    private void c_Grid_AchievementItem_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Grid grid)
        {
            grid.BorderThickness = new Thickness(2);
            grid.BorderBrush = ItemGridSelectedBorderBrush;
            grid.Background = ItemGridPressedBackgroundBrush;
            SelectedAchievementItemGrid = grid;
        }
    }


    private void c_Grid_AchievementItem_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Grid grid)
        {
            grid.BorderThickness = new Thickness(1);
            grid.BorderBrush = ItemGridNonSelectedBorderBrush;
            grid.Background = ItemGridNonPressedBackgroundBrush;
        }
    }


    [ObservableProperty]
    private Border selectedAchievementGoalBorder;

    // #2E3643
    private SolidColorBrush GoalNonSelectedBackgroundBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x2E, 0x36, 0x43));
    // #42484F
    private SolidColorBrush GoalNonSelectedBorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x42, 0x48, 0x4F));
    // #ECE5D8
    private SolidColorBrush GoalNonSelectedNameBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xEC, 0xE5, 0xD8));
    // #D3BC8E
    private SolidColorBrush GoalNonSelectedProgressBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xD3, 0xBC, 0x8E));

    // #E5DED2
    private SolidColorBrush GoalSelectedBackgroundBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xE5, 0xDE, 0xD2));
    // #CCC6BB
    private SolidColorBrush GoalSelectedBorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xCC, 0xC6, 0xBB));
    // #2E3643
    private SolidColorBrush GoalSelectedNameBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x2E, 0x36, 0x43));
    // #A6947C
    private SolidColorBrush GoalSelectedProgressBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xA6, 0x94, 0x7C));


    partial void OnSelectedAchievementGoalBorderChanging(Border value)
    {
        if (selectedAchievementGoalBorder != null)
        {
            var border = selectedAchievementGoalBorder;
            if (border.Child is Grid grid)
            {
                border.Scale = Vector3.One;
                border.Background = GoalNonSelectedBackgroundBrush;
                grid.BorderBrush = GoalNonSelectedBorderBrush;
                if (grid.Children.FirstOrDefault() is CachedImage image)
                {
                    image.Source = mapTitleBgNonSelected;
                }
                if (grid.Children.Skip(1).FirstOrDefault() is StackPanel stackPanel)
                {
                    if (stackPanel.Children.FirstOrDefault() is TextBlock name)
                    {
                        name.Foreground = GoalNonSelectedNameBrush;
                    }
                    if (stackPanel.Children.Skip(1).FirstOrDefault() is TextBlock progress)
                    {
                        progress.Foreground = GoalNonSelectedProgressBrush;
                    }
                }
            }
        }
        if (value != null)
        {
            var border = value;
            if (border.Child is Grid grid)
            {
                border.Background = GoalSelectedBackgroundBrush;
                grid.BorderBrush = GoalSelectedBorderBrush;
                if (grid.Children.FirstOrDefault() is CachedImage image)
                {
                    image.Source = mapTitleBgSelected;
                }
                if (grid.Children.Skip(1).FirstOrDefault() is StackPanel stackPanel)
                {
                    if (stackPanel.Children.FirstOrDefault() is TextBlock name)
                    {
                        name.Foreground = GoalSelectedNameBrush;
                    }
                    if (stackPanel.Children.Skip(1).FirstOrDefault() is TextBlock progress)
                    {
                        progress.Foreground = GoalSelectedProgressBrush;
                    }
                }
            }
        }
    }



    private void c_ListView_AchievementGoal_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        //var add = e.AddedItems.FirstOrDefault();
        //if (add != null)
        //{
        //    var container = c_ListView_AchievementGoal.ContainerFromItem(add);
        //    if (container is ListViewItem item)
        //    {
        //        if (item.ContentTemplateRoot is Border border)
        //        {
        //            SelectedAchievementGoalBorder = border;
        //        }
        //    }
        //}
        //if (add is AchievementPageModel_Goal goal)
        //{
        //    SelectedGoal = goal;
        //    Achievements = goal.Items;
        //    //SelectedAchievementItemGrid = null;
        //    c_Grid_GoalReward.Visibility = Visibility.Visible;
        //}
    }


    private void c_Border_Achievementgoal_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Border border)
        {
            border.CenterPoint = new Vector3((float)border.ActualWidth / 2, (float)border.ActualHeight / 2, 1);
            border.Scale = new Vector3(1 + (16 / (float)border.ActualWidth), 1.1f, 1);
        }
    }

    private void c_Border_Achievementgoal_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Border border)
        {
            if (border != SelectedAchievementGoalBorder)
            {
                border.Scale = Vector3.One;
            }
        }
    }





    #endregion


}
