using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Xunkong.Hoyolab;
using Xunkong.Hoyolab.Account;
using Xunkong.Hoyolab.DailyNote;

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


    [ObservableProperty]
    private HoyolabUserInfo _HoyolabUserInfo;

    [ObservableProperty]
    private GenshinRoleInfo _GenshinRoleInfo;

    [ObservableProperty]
    private DailyNoteInfo _DailyNoteInfo;

    [ObservableProperty]
    private bool _Error;

    [ObservableProperty]
    private string _ErrorMessage;

    [ObservableProperty]
    private bool _NeedVerification;


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
        catch { }
    }




    [RelayCommand]
    private async Task RefreshAsync()
    {
        try
        {
            var client = ServiceProvider.GetService<HoyolabService>()!;
            var dailynote = await client.GetDailyNoteAsync(GenshinRoleInfo, true);
            DailyNoteInfo = dailynote!;
            ErrorMessage = "";
            Error = false;
            NeedVerification = false;
        }
        catch (HoyolabException ex)
        {
            if (ex.ReturnCode != 1034)
            {
                NeedVerification = false;
            }
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            NeedVerification = false;
            ErrorMessage = ex.Message;
        }
    }




    [RelayCommand]
    private void Verify()
    {
        try
        {
            if (Flyout_WebBridge.Content is null)
            {
                Flyout_WebBridge.Content = new DailyNoteWebBridge(HoyolabUserInfo, GenshinRoleInfo);
            }
            FlyoutBase.ShowAttachedFlyout(c_Grid_Base);
        }
        catch { }
    }






    /// <summary>
    /// 固定到开始菜单
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task PinToStartMenuAsync()
    {
        //if (Environment.OSVersion.Version >= new Version("10.0.22000.0"))
        //{
        //    NotificationProvider.Warning("您的操作系统不支持开始菜单磁贴", 3000);
        //    return;
        //}
        try
        {
            if (DailyNoteInfo is not null)
            {
                var result = await SecondaryTileProvider.RequestPinTileAsync(DailyNoteInfo);
                if (result)
                {
                    TaskSchedulerService.RegisterForRefreshTile();
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
