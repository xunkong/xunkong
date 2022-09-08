using Microsoft.UI.Xaml.Controls;
using Xunkong.Hoyolab.Account;
using Xunkong.Hoyolab.DailyNote;
using Xunkong.Hoyolab.TravelNotes;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Controls;

[INotifyPropertyChanged]
public sealed partial class DailyNoteThumbCard : UserControl
{


    public DailyNoteThumbCard()
    {
        this.InitializeComponent();
        Loaded += DailyNoteThumbCard_Loaded;
    }


    public HoyolabUserInfo HoyolabUserInfo { get; set; }


    public GenshinRoleInfo GenshinRoleInfo { get; set; }


    public DailyNoteInfo DailyNoteInfo { get; set; }


    public TravelNotesDayData TravelNotesDayData { get; set; }


    public bool Error { get; set; }


    public string ErrorMessage { get; set; }


    private async void DailyNoteThumbCard_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (Error)
        {
            return;
        }
        try
        {
            if (DailyNoteInfo != null)
            {

                if (DailyNoteInfo.CurrentResin > 150 ||
                    DailyNoteInfo.CurrentHomeCoin > 0.95 * DailyNoteInfo.MaxHomeCoin ||
                    !DailyNoteInfo.IsExtraTaskRewardReceived ||
                    (DailyNoteInfo.Transformer?.RecoveryTime?.Reached ?? false))
                {
                    await Task.Delay(500);
                    CautionStoryBoard.Begin();
                }
            }
        }
        catch (Exception ex)
        {

        }
    }


    /// <summary>
    /// 固定到开始菜单
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task PinToStartMenuAsync()
    {
        if (Environment.OSVersion.Version >= new Version("10.0.22000.0"))
        {
            NotificationProvider.Warning("您的操作系统不支持开始菜单磁贴", 3000);
            return;
        }
        try
        {
            if (DailyNoteInfo is not null)
            {
                var result = await SecondaryTileProvider.RequestPinTileAsync(DailyNoteInfo);
                if (result)
                {
                    TaskSchedulerService.RegisterForRefreshTile(result);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "固定磁贴到开始菜单");
            NotificationProvider.Error(ex, "固定磁贴到开始菜单");
        }
    }



    /// <summary>
    /// 复制 cookie
    /// </summary>
    [RelayCommand]
    private void CopyCookie()
    {
        try
        {
            if (GenshinRoleInfo is not null)
            {
                ClipboardHelper.SetText(GenshinRoleInfo.Cookie);
                NotificationProvider.Success("已复制", 1500);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "复制 cookie");
            NotificationProvider.Error(ex, "复制 cookie");
        }
    }


}
