using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Xunkong.Hoyolab.SpiralAbyss;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class SpiralAbyssPage : Page
{

    static SpiralAbyssPage()
    {
        TypeAdapterConfig<SpiralAbyssInfo, SpiralAbyssPageModel_AbyssInfo>.NewConfig()
                                                                          .Map(dest => dest.DamageRank, src => src.DamageRank.FirstOrDefault())
                                                                          .Map(dest => dest.DefeatRank, src => src.DefeatRank.FirstOrDefault())
                                                                          .Map(dest => dest.EnergySkillRank, src => src.EnergySkillRank.FirstOrDefault())
                                                                          .Map(dest => dest.NormalSkillRank, src => src.NormalSkillRank.FirstOrDefault())
                                                                          .Map(dest => dest.TakeDamageRank, src => src.TakeDamageRank.FirstOrDefault())
                                                                          .Map(dest => dest.Floors, src => src.Floors.Where(x => x.Index > 8).ToList());
    }


    private readonly HoyolabService _hoyolabService;


    public SpiralAbyssPage()
    {
        this.InitializeComponent();
        NavigationCacheMode = AppSetting.GetValue<bool>(SettingKeys.EnableNavigationCache) ? NavigationCacheMode.Enabled : NavigationCacheMode.Disabled;
        _hoyolabService = ServiceProvider.GetService<HoyolabService>()!;
        WeakReferenceMessenger.Default.Register<SelectedGameRoleChangedMessage>(this, (_, e) => InitializeData());
        Loaded += SpiralAbyssPage_Loaded;
        //Unloaded += SpiralAbyssPage_Unloaded;
    }



    [ObservableProperty]
    private List<SpiralAbyssPageModel_LeftPanel> leftPanels;

    [ObservableProperty]
    private SpiralAbyssPageModel_AbyssInfo? selectedAbyssInfo;

    private Dictionary<int, SpiralAbyssPageModel_AbyssInfo> abyssDic = new();


    private async void SpiralAbyssPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (LeftPanels is null)
        {
            await Task.Delay(60);
            InitializeData();
        }
    }


    private void SpiralAbyssPage_Unloaded(object sender, RoutedEventArgs e)
    {
        WeakReferenceMessenger.Default.Unregister<SelectedGameRoleChangedMessage>(this);
    }


    public void InitializeData()
    {
        try
        {
            var role = _hoyolabService.GetLastSelectedOrFirstGenshinRoleInfo();
            if (role == null)
            {
                return;
            }
            LeftPanels = _hoyolabService.GetSpiralAbyssInfos(role.Uid);
            var firstId = LeftPanels?.FirstOrDefault()?.Id ?? 0;
            SelectedAbyssInfo = _hoyolabService.GetSpiralAbyssInfo(firstId)?.Adapt<SpiralAbyssPageModel_AbyssInfo>();
            if (SelectedAbyssInfo != null)
            {
                abyssDic.TryAdd(firstId, SelectedAbyssInfo);
                _GridView_LeftPanel.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex, "初始化深渊页面");
            Logger.Error(ex, "初始化深渊页面");
        }
    }


    [RelayCommand]
    public async Task GetSpiralAbyssDataAsync()
    {
        try
        {
            var role = _hoyolabService.GetLastSelectedOrFirstGenshinRoleInfo();
            if (role is null)
            {
                NotificationProvider.Warning("没有找到账号信息");
                return;
            }
            var last = await _hoyolabService.GetSpiralAbyssInfoAsync(role, 2);
            var current = await _hoyolabService.GetSpiralAbyssInfoAsync(role, 1);
            OperationHistory.AddToDatabase("GetAbyss", role.Uid.ToString());
            SelectedAbyssInfo = null;
            abyssDic.Clear();
            InitializeData();
            NotificationProvider.Success($"已获取 {role.Nickname} 最新的深渊记录");
            _hoyolabService.SaveAvatarDetailsAsync(role);
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex, "获取深渊数据");
            Logger.Error(ex, "获取深渊数据");
        }
    }



    private void _GridView_LeftPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_GridView_LeftPanel.SelectedItem is SpiralAbyssPageModel_LeftPanel leftInfo)
        {
            try
            {
                var id = leftInfo.Id;
                if (abyssDic.TryGetValue(id, out var info))
                {
                    SelectedAbyssInfo = info;
                }
                else
                {
                    var originInfo = _hoyolabService.GetSpiralAbyssInfo(id);
                    if (originInfo is not null)
                    {
                        SelectedAbyssInfo = originInfo.Adapt<SpiralAbyssPageModel_AbyssInfo>();
                        abyssDic.TryAdd(id, SelectedAbyssInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                NotificationProvider.Error(ex, "切换显示的深渊数据");
                Logger.Error(ex, "切换显示的深渊数据");
            }
        }
    }



}
