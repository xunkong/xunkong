using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;
using Xunkong.Hoyolab.Account;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class WebViewPage : Page
{


    public record NavigateParameter(string Key, string Url, object? Data = null);


    public WebViewPage()
    {
        this.InitializeComponent();
        _WebView2.NavigationStarting += _WebView2_NavigationStarting;
    }



    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        try
        {
            if (e.Parameter is NavigateParameter parameter)
            {
                if (Uri.TryCreate(parameter.Url, UriKind.Absolute, out var uri))
                {
                    if (parameter.Key == "BirthdayStar" && parameter.Data is GenshinRoleInfo role)
                    {
                        await _WebView2.EnsureCoreWebView2Async();
                        var manager = _WebView2.CoreWebView2.CookieManager;
                        var cookies = await manager.GetCookiesAsync(parameter.Url);
                        foreach (var cookie in cookies)
                        {
                            manager.DeleteCookie(cookie);
                        }
                        foreach (var item in ParseCookie(role.Cookie!))
                        {
                            manager.AddOrUpdateCookie(manager.CreateCookie(item.Key, item.Value, ".mihoyo.com", "/"));
                        }
                    }
                    _WebView2.Source = uri;
                    return;
                }
            }
            _WebView2.Source = new Uri("https://xunkong.cc");
        }
        catch { }
    }



    private List<(string Key, string Value)> ParseCookie(string cookie)
    {
        var list = new List<(string Key, string Value)>();
        var cookies = cookie.Split(';');
        if (cookies != null)
        {
            foreach (var item in cookies)
            {
                if (item.Contains('='))
                {
                    var key = item.Split("=")[0].Trim();
                    var value = item.Split("=")[1].Trim();
                    if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value))
                    {
                        list.Add((key, value));
                    }
                }
            }
        }
        return list;
    }



    private void _WebView2_NavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
    {
        _TextBox_Address.Text = args.Uri.ToString();
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
            if (!address.StartsWith("https://") && !address.StartsWith("http://"))
            {
                address = $"https://{address}";
            }
            try
            {
                if (Uri.TryCreate(address, UriKind.Absolute, out var uri))
                {
                    if (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
                    {
                        _WebView2.Source = uri;
                    }
                }
            }
            catch { }
        }
    }



}
