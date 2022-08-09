using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Web;
using Windows.ApplicationModel.Activation;
using Windows.Foundation.Collections;
using Windows.System;
using Xunkong.Hoyolab.Account;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class GetCookiePage : Page
{


    private readonly HoyolabService _hoyolabService;


    private ProtocolForResultsOperation? resultOperation;


    [ObservableProperty]
    private List<GetCookiePageModel_HoyolabAndGenshinUserInfo> users;


    public GetCookiePage()
    {
        this.InitializeComponent();
        _hoyolabService = ServiceProvider.GetService<HoyolabService>()!;
        Loaded += GetCookiePage_Loaded;
    }



    public GetCookiePage(IProtocolActivatedEventArgs args)
    {
        this.InitializeComponent();
        ResizeToolWindow();
        InitializeTitle(args);
        Loaded += GetCookiePage_Loaded;
        _hoyolabService = ServiceProvider.GetService<HoyolabService>()!;

    }

    
    private void ResizeToolWindow()
    {
        var uiScale = ToolWindow.Current.UIScale;
        var width = (int)(360 * uiScale);
        var height = (int)(480 * uiScale);
        var workArea = ToolWindow.Current.DisplayArea.WorkArea;
        var left = (workArea.Width - width) / 2;
        var top = (workArea.Height - height) / 2;
        ToolWindow.Current.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(left, top, width, height));
    }


    private void InitializeTitle(IProtocolActivatedEventArgs args)
    {
        if (args is ProtocolActivatedEventArgs args1)
        {
            var qc = HttpUtility.ParseQueryString(args1.Uri.Query);
            var caller = qc["caller"];
            c_TextBlock_Title.Text = "复制 Cookie";
            c_TextBlock_Caller.Text = $"调用方：{(string.IsNullOrWhiteSpace(caller) ? "未知" : caller)}";
        }
        if (args is ProtocolForResultsActivatedEventArgs args2)
        {
            // 无法通过 LaunchUriForResultsAsync 打开 win32 程序
            resultOperation = args2.ProtocolForResultsOperation;
            c_Grid_Button.Visibility = Visibility.Visible;
            c_GridView_Cookies.SelectionMode = ListViewSelectionMode.Single;
            var qc = HttpUtility.ParseQueryString(args2.Uri.Query);
            var caller = qc["caller"];
            c_TextBlock_Title.Text = "选择 Cookie 并发送给调用方";
            c_TextBlock_Caller.Text = $"调用方：{(string.IsNullOrWhiteSpace(caller) ? "未知" : caller)}";
        }
    }


    private void GetCookiePage_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var users = _hoyolabService.GetHoyolabUserInfoList();
            var roles = _hoyolabService.GetGenshinRoleInfoList();
            Users = users.Join(roles, x => x.Cookie, y => y.Cookie, (x, y) => new GetCookiePageModel_HoyolabAndGenshinUserInfo(x, y)).ToList();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "初始化获取 Cookie 页面");
        }
    }



    private async void c_Button_CopyCookie_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            if (button.Tag is GetCookiePageModel_HoyolabAndGenshinUserInfo info)
            {
                try
                {
                    ClipboardHelper.SetText(info.GenshinRoleInfo.Cookie);
                    button.Content = "\xE8FB";
                    await Task.Delay(3000);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "复制 Cookie");
                }
                finally
                {
                    button.Content = "\xE8C8";
                }
            }
        }
    }



    [RelayCommand]
    private void OK()
    {
        if (c_GridView_Cookies.SelectedItem is GetCookiePageModel_HoyolabAndGenshinUserInfo info)
        {
            if (resultOperation != null)
            {
                var cookie = info.GenshinRoleInfo.Cookie;
                var valueSet = new ValueSet();
                valueSet.Add("IsSuccess", true);
                valueSet.Add("Message", "OK");
                valueSet.Add("Data", cookie);
                resultOperation.ReportCompleted(valueSet);
                ToolWindow.Current.Close();
            }
        }
    }


    [RelayCommand]
    private void Cancel()
    {
        if (resultOperation != null)
        {
            var valueSet = new ValueSet();
            valueSet.Add("IsSuccess", false);
            valueSet.Add("Message", "用户拒绝提供 Cookie");
            resultOperation.ReportCompleted(valueSet);
        }
        ToolWindow.Current.Close();
    }




}



public class GetCookiePageModel_HoyolabAndGenshinUserInfo
{
    public GetCookiePageModel_HoyolabAndGenshinUserInfo(HoyolabUserInfo hoyolabUserInfo, GenshinRoleInfo genshinRoleInfo)
    {
        HoyolabUserInfo = hoyolabUserInfo;
        GenshinRoleInfo = genshinRoleInfo;
    }

    public HoyolabUserInfo HoyolabUserInfo { get; set; }

    public GenshinRoleInfo GenshinRoleInfo { get; set; }
}