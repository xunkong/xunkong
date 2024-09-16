using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class LoginPage : Page
{

    private const string URL = "https://www.miyoushe.com/ys/";


    public LoginPage()
    {
        this.InitializeComponent();
        Loaded += LoginPage_Loaded;
        _WebView2.NavigationCompleted += _WebView2_NavigationCompleted;
    }


    private async void LoginPage_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await _WebView2.EnsureCoreWebView2Async();
            var manager = _WebView2.CoreWebView2.CookieManager;
            var cookies = await manager.GetCookiesAsync(URL);
            foreach (var item in cookies)
            {
                manager.DeleteCookie(item);
            }
            _WebView2.CoreWebView2.Navigate(URL);
            _TeachTip_Ok.IsOpen = true;
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex, "初始化登录页面");
            Logger.Error(ex, "初始化登录页面");
        }
    }



    private async void _WebView2_NavigationCompleted(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
    {
        try
        {
            if (args.IsSuccess && sender.Source.ToString() == URL)
            {
                const string js = """
                    var removeQRIntervalId = setInterval(RemoveAppQRCode, 100);
                    function RemoveAppQRCode() {
                        var c1 = document.getElementsByClassName('mhy-side-section mhy-sem-section');
                        var removed = false;
                        if (c1.length > 0) {
                            c1[0].remove();
                            removed = true;
                        }
                        var c2 = document.getElementsByClassName('mhy-ad-container swiper-container swiper-container-initialized swiper-container-horizontal');
                        if (c2.length > 0) {
                            c2[0].remove();
                            removed = true;
                        }
                        if (removed) {
                            clearInterval(removeQRIntervalId);
                            removeQRIntervalId = null;
                            var c3 = document.getElementsByClassName('header__avatarwrp')
                            if (c3.length > 0) {
                                c3[0].click();
                            }
                        }
                    }
                    """;
                await _WebView2.CoreWebView2.ExecuteScriptAsync(js);
            }
        }
        catch { }
    }



    [RelayCommand]
    private void GoBack()
    {
        if (_WebView2.CanGoBack)
        {
            _WebView2.GoBack();
        }
    }


    [RelayCommand]
    private void GoForward()
    {
        if (_WebView2.CanGoForward)
        {
            _WebView2.GoForward();
        }
    }


    [RelayCommand]
    private void Refresh()
    {
        _WebView2.Reload();
    }


    private void _TextBox_Address_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            var address = _TextBox_Address.Text;
            if (!(address.StartsWith("https://") || address.StartsWith("http://")))
            {
                address = $"https://{address}";
            }
            try
            {
                if (Uri.TryCreate(address, UriKind.RelativeOrAbsolute, out var uri))
                {
                    if (uri.Scheme == Uri.UriSchemeHttps)
                    {
                        _WebView2.Source = uri;
                    }
                }
            }
            catch { }
        }
    }



    private void _WebView2_NavigationStarting(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs args)
    {
        _TextBox_Address.Text = args.Uri.ToString();
    }


    [RelayCommand]
    private async Task FinishAndAddCookie()
    {
        try
        {
            OperationHistory.AddToDatabase("Login", "Web");
            Logger.TrackEvent("Login", "Type", "Web");
            var manager = _WebView2.CoreWebView2.CookieManager;
            var cookies = await manager.GetCookiesAsync(URL);
            var str = string.Join(";", cookies.Select(x => $"{x.Name}={x.Value}"));
            await MainPage.Current.AddCookieAsync(str);
            NotificationProvider.Success("已添加账号");
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex, "添加 Cookie");
            Logger.Error(ex, "添加 Cookie");
        }
    }




}
