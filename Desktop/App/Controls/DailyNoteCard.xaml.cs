using Microsoft.UI.Xaml.Controls;
using Xunkong.Hoyolab.Account;
using Xunkong.Hoyolab.DailyNote;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Controls;

[INotifyPropertyChanged]
public sealed partial class DailyNoteCard : UserControl
{


    public DailyNoteCard()
    {
        this.InitializeComponent();
    }



    [ObservableProperty]
    private HoyolabUserInfo hoyolabUserInfo;


    [ObservableProperty]
    private GenshinRoleInfo genshinRoleInfo;


    [ObservableProperty]
    private DailyNoteInfo? dailyNoteInfo;

    [ObservableProperty]
    public string updateTimeAgoText;


    [ObservableProperty]
    private bool error = true;


    [ObservableProperty]
    private string? errorMessage;



    /// <summary>
    /// 固定到开始菜单
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task PinToStartMenuAsync()
    {
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



    private string GetUpdateTimeAgoText()
    {
        if (DailyNoteInfo != null)
        {
            var span = DateTimeOffset.Now - DailyNoteInfo.UpdateTime;
            if (span.Hours > 0)
            {
                return $"更新于 {span.Hours} 小时前";
            }
            if (span.Minutes > 0)
            {
                return $"更新于 {span.Minutes} 分钟前";
            }
            if (span.Seconds > 0)
            {
                return $"更新于 {span.Seconds} 秒前";
            }
        }
        return "";
    }

    private void Grid_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        UpdateTimeAgoText = GetUpdateTimeAgoText();
    }
}
