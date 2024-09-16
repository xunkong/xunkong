using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using MiniExcelLibs;
using Syncfusion.Licensing;
using System.Diagnostics;
using System.Net.Http;
using Windows.ApplicationModel;
using Xunkong.Hoyolab;
using Xunkong.Hoyolab.TravelNotes;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class TravelNotesPage : Page
{


    static TravelNotesPage()
    {
        SyncfusionLicenseProvider.RegisterLicense("NjEwMjc4QDMyMzAyZTMxMmUzMFkyTEYzc3JuY2hOVXk1azE5d1p1K1NPaFQ4TFF4bU5zYXR2ZUdBQmorc2c9");
        TypeAdapterConfig<TravelNotesAwardItem, TravelNotesPage_TravelNotesAwardItem>.NewConfig().Map(dest => dest.Time, src => src.Time.ToString("yyyy-MM-dd HH:mm:ss"));
    }


    private readonly HoyolabService _hoyolabService;


    public TravelNotesPage()
    {
        this.InitializeComponent();
        NavigationCacheMode = AppSetting.GetValue<bool>(SettingKeys.EnableNavigationCache) ? NavigationCacheMode.Enabled : NavigationCacheMode.Disabled;
        _hoyolabService = ServiceProvider.GetService<HoyolabService>()!;
        Loaded += TravelNotesPage_Loaded;
    }

    private async void TravelNotesPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (DayStats is null)
        {
            await Task.Delay(60);
            await InitializeDataAsync();
        }
    }

    private void SfCartesianChart_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        e.Handled = true;
    }


    private bool _IsLoading;
    public bool IsLoading
    {
        get => _IsLoading;
        set => SetProperty(ref _IsLoading, value);
    }




    private TravelNotesPage_DayOrMonthStats? _DayStats;
    public TravelNotesPage_DayOrMonthStats? DayStats
    {
        get => _DayStats;
        set => SetProperty(ref _DayStats, value);
    }



    private TravelNotesPage_DayOrMonthStats? _MonthStats;
    public TravelNotesPage_DayOrMonthStats? MonthStats
    {
        get => _MonthStats;
        set => SetProperty(ref _MonthStats, value);
    }




    private List<TravelNotesPrimogemsMonthGroupStats>? _PrimogemsGroup;
    public List<TravelNotesPrimogemsMonthGroupStats>? PrimogemsGroup
    {
        get => _PrimogemsGroup;
        set => SetProperty(ref _PrimogemsGroup, value);
    }



    private List<TravelNotesPage_DayData> _RecentDayData;
    public List<TravelNotesPage_DayData> RecentDayData
    {
        get => _RecentDayData;
        set => SetProperty(ref _RecentDayData, value);
    }





    private List<TravelNotesPage_MonthData> _AllMonthData;
    public List<TravelNotesPage_MonthData> AllMonthData
    {
        get => _AllMonthData;
        set => SetProperty(ref _AllMonthData, value);
    }




    private List<TravelNotesAwardItem> _PrimogemsAwardItems;
    public List<TravelNotesAwardItem> PrimogemsAwardItems
    {
        get => _PrimogemsAwardItems;
        set => SetProperty(ref _PrimogemsAwardItems, value);
    }




    private List<TravelNotesAwardItem> _MoraAwardItems;
    public List<TravelNotesAwardItem> MoraAwardItems
    {
        get => _MoraAwardItems;
        set => SetProperty(ref _MoraAwardItems, value);
    }




    [RelayCommand]
    public async Task InitializeDataAsync()
    {
        IsLoading = true;
        try
        {
            var role = _hoyolabService.GetLastSelectedOrFirstGenshinRoleInfo();
            if (role is null)
            {
                NotificationProvider.Warning("没有找到已添加的账号");
                return;
            }
            else
            {
                try
                {
                    var summary = await _hoyolabService.GetTravelNotesSummaryAsync(role, DateTime.UtcNow.AddHours(4).Month);
                    DayStats = summary?.DayData?.Adapt<TravelNotesPage_DayOrMonthStats>();
                    MonthStats = summary?.MonthData?.Adapt<TravelNotesPage_DayOrMonthStats>();
                    PrimogemsGroup = summary?.MonthData?.PrimogemsGroupBy;
                }
                catch (Exception ex) when (ex is HttpRequestException or HoyolabException)
                {
                    NotificationProvider.Error(ex, "初始化旅行札记页面");
                    Logger.Error(ex, "初始化旅行札记页面");
                }
                var minTime = DateTime.UtcNow.AddDays(-30).AddHours(12);
                var recentDays = _hoyolabService.GetTravelNotesAwardItems(role.Uid, minTime, null).ToList();
                var group = recentDays.GroupBy(x => x.Time.Date).OrderBy(x => x.Key);
                var dayDatas = new List<TravelNotesPage_DayData>(30);
                foreach (var item in group)
                {
                    var primogems = item.Where(x => x.Type == TravelNotesAwardType.Primogems).Sum(x => x.Number);
                    var mora = item.Where(x => x.Type == TravelNotesAwardType.Mora).Sum(x => x.Number);
                    var dayData = new TravelNotesPage_DayData(item.Key.ToString("MM-dd"), primogems, mora);
                    dayDatas.Add(dayData);
                }
                RecentDayData = dayDatas;
                AllMonthData = _hoyolabService.GetTravelNotesMonthData(role.Uid).OrderBy(x => x.Year).ThenBy(x => x.Month).Adapt<List<TravelNotesPage_MonthData>>();
                PrimogemsAwardItems = recentDays.Where(x => x.Type == TravelNotesAwardType.Primogems).ToList();
                MoraAwardItems = recentDays.Where(x => x.Type == TravelNotesAwardType.Mora).ToList();
            }
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex, "初始化旅行札记页面");
            Logger.Error(ex, "初始化旅行札记页面");
        }
        finally
        {
            IsLoading = false;
        }
    }




    [RelayCommand]
    private async Task GetTravelRecordDetailAsync(bool onlyCurrentMonth)
    {
        IsLoading = true;
        try
        {
            var role = _hoyolabService.GetLastSelectedOrFirstGenshinRoleInfo();
            if (role is null)
            {
                NotificationProvider.Warning("没有找到已添加的账号");
                return;
            }
            else
            {
                var now = DateTime.UtcNow.AddHours(4);
                var month = now.Month;
                await _hoyolabService.GetTravelNotesSummaryAsync(role, month);
                var primogemsAddCount = await _hoyolabService.GetTravelRecordDetailAsync(role, month, TravelNotesAwardType.Primogems);
                var moraAddCount = await _hoyolabService.GetTravelRecordDetailAsync(role, month, TravelNotesAwardType.Mora);
                NotificationProvider.Success($"{role.Nickname} 的 {month} 月旅行札记新增原石收入记录 {primogemsAddCount} 条、摩拉收入记录 {moraAddCount} 条", 10000);
                if (!onlyCurrentMonth)
                {
                    month = now.AddMonths(-1).Month;
                    await _hoyolabService.GetTravelNotesSummaryAsync(role, month);
                    primogemsAddCount = await _hoyolabService.GetTravelRecordDetailAsync(role, month, TravelNotesAwardType.Primogems);
                    moraAddCount = await _hoyolabService.GetTravelRecordDetailAsync(role, month, TravelNotesAwardType.Mora);
                    NotificationProvider.Success($"{role.Nickname} 的 {month} 月旅行札记新增原石收入记录 {primogemsAddCount} 条、摩拉收入记录 {moraAddCount} 条", 10000);
                    month = now.AddMonths(-2).Month;
                    await _hoyolabService.GetTravelNotesSummaryAsync(role, month);
                    primogemsAddCount = await _hoyolabService.GetTravelRecordDetailAsync(role, month, TravelNotesAwardType.Primogems);
                    moraAddCount = await _hoyolabService.GetTravelRecordDetailAsync(role, month, TravelNotesAwardType.Mora);
                    NotificationProvider.Success($"{role.Nickname} 的 {month} 月旅行札记新增原石收入记录 {primogemsAddCount} 条、摩拉收入记录 {moraAddCount} 条", 10000);
                }
                OperationHistory.AddToDatabase("GetTravelRecordDetail", "OnlyCurrentMonth", onlyCurrentMonth);
                Logger.TrackEvent("GetTravelRecordDetail", "OnlyCurrentMonth", onlyCurrentMonth.ToString());
            }
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex, "获取旅行札记详细数据");
            Logger.Error(ex, "获取旅行札记详细数据");
        }
        finally
        {
            IsLoading = false;
        }
        await InitializeDataAsync();
    }


    [RelayCommand]
    private async Task ExportDataAsync()
    {
        try
        {
            var uid = _hoyolabService.GetLastSelectedOrFirstGenshinRoleInfo()?.Uid;
            if (uid == 0)
            {
                return;
            }
            var param = new { Uid = uid };
            using var dapper = DatabaseProvider.CreateConnection();
            var monthdatas = dapper.Query<TravelNotesMonthData>("SELECT Uid,Year,Month,CurrentPrimogems,CurrentMora,LastPrimogems,LastMora FROM TravelNotesMonthData WHERE Uid=@Uid ORDER BY Year,Month;", param);
            var primogemsData = dapper.Query<TravelNotesAwardItem>("SELECT * FROM TravelNotesAwardItem WHERE Uid=@Uid AND Type=1 ORDER BY Time ASC, Id DESC;", param);
            var moraData = dapper.Query<TravelNotesAwardItem>("SELECT * FROM TravelNotesAwardItem WHERE Uid=@Uid AND Type=2 ORDER BY Time ASC, Id DESC;", param);
            if (!monthdatas.Any() && !primogemsData.Any() && !moraData.Any())
            {
                NotificationProvider.Warning($"Uid {uid} 没有任何旅行札记的数据");
                return;
            }
            NotificationProvider.Information("导出中。。。");
            var obj = new
            {
                MonthData = monthdatas.Adapt<List<TravelNotesPage_TravelNotesMonthData>>(),
                PrimogemsData = primogemsData.Adapt<List<TravelNotesPage_TravelNotesAwardItem>>(),
                MoraData = moraData.Adapt<List<TravelNotesPage_TravelNotesAwardItem>>()
            };
            var filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $@"Xunkong\Export\TravelNotes\TravelNotes_{uid}_{DateTimeOffset.Now:yyyyMMdd_HHmmss}.xlsx");
            Directory.CreateDirectory(Path.GetDirectoryName(filename)!);
            using var fs = File.Open(filename, FileMode.CreateNew);
            var templatePath = Path.Combine(Package.Current.InstalledLocation.Path, @"Assets\Others\TravelNotesTemplate.xlsx");
            await MiniExcel.SaveAsByTemplateAsync(fs, templatePath, obj);
            void action() => Process.Start(new ProcessStartInfo { FileName = Path.GetDirectoryName(filename), UseShellExecute = true });
            NotificationProvider.ShowWithButton(InfoBarSeverity.Success, "导出完成", null, "打开文件夹", action);
            OperationHistory.AddToDatabase("ExportTravelRecordDetail", uid.ToString());
            Logger.TrackEvent("ExportTravelRecordDetail");
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex, "导出旅行札记");
            Logger.Error(ex, "导出旅行札记");
        }
    }




}


