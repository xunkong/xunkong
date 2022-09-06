using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Text;
using Windows.ApplicationModel;
using Xunkong.Hoyolab.Activity;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Controls;

[INotifyPropertyChanged]
public sealed partial class AnnouncementContentViewer : UserControl
{
    public AnnouncementContentViewer()
    {
        this.InitializeComponent();
        Loaded += AnnouncementContentViewer_Loaded;
    }


    [ObservableProperty]
    private Announcement announce;

    private string? css;

    private void AnnouncementContentViewer_Loaded(object sender, RoutedEventArgs e)
    {
        LoadHtmlContentAsync();
    }



    private async void LoadHtmlContentAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Announce?.Content))
            {
                return;
            }
            var sb = new StringBuilder();
            if (css is null)
            {
                css = await File.ReadAllTextAsync(Path.Combine(Package.Current.InstalledPath, @"Assets\Others\github-markdown_5.1.0.css"));
            }
            sb.AppendLine("""
                <head>
                <base target="_blank">
                <meta name="color-scheme" content="light dark">
                <style>
                body::-webkit-scrollbar {display: none;}
                """);
            sb.AppendLine(css);
            sb.AppendLine("""
                </style>
                </head>
                <body>
                <article class="markdown-body" style="background-color: transparent;">
                """);
            sb.AppendLine($"<h3>{announce.Title}</h3>");
            if (!string.IsNullOrWhiteSpace(announce.Banner))
            {
                sb.AppendLine($"""<img width="100%" src="{announce.Banner}" />""");
                sb.AppendLine("<br>");
                sb.AppendLine("<br>");
            }
            sb.AppendLine(announce.Content);
            sb.AppendLine("</article>");
            sb.AppendLine("</body>");
            sb.Replace("style=\"color:rgba(85,85,85,1)\"", "");
            sb.Replace("style=\"background-color: rgb(255, 215, 185);\"", "");
            sb.Replace("style=\"background-color: rgb(254, 245, 231);\"", "");
            sb.Replace("&lt;t class=\"t_lc\"&gt;", "");
            sb.Replace("&lt;t class=\"t_gl\"&gt;", "");
            sb.Replace("&lt;/t&gt;", "");
            await webview.EnsureCoreWebView2Async();
            webview.CoreWebView2.Settings.AreDevToolsEnabled = false;
            webview.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            webview.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
            webview.NavigateToString(sb.ToString());
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "活动公告");
        }
    }


}
