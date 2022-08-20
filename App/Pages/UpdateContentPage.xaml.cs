using Microsoft.UI.Xaml;
using Octokit;
using Windows.ApplicationModel;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class UpdateContentPage : Microsoft.UI.Xaml.Controls.Page
{


    private new string Tag => XunkongEnvironment.AppVersion.ToString(3);


    public UpdateContentPage()
    {
        this.InitializeComponent();
        Loaded += UpdateContentPage_Loaded;
    }

    private async void UpdateContentPage_Loaded(object sender, RoutedEventArgs e)
    {
        await InitializeCommand.ExecuteAsync(null);
    }


    [RelayCommand]
    private async Task InitializeAsync()
    {
        try
        {
            var client = new GitHubClient(new ProductHeaderValue("xunkong"));
            var release = await client.Repository.Release.Get("xunkong", "xunkong", Tag);
            var html = Markdig.Markdown.ToHtml(release.Body);
            var css = await File.ReadAllTextAsync(Path.Combine(Package.Current.InstalledPath, @"Assets\Others\github-markdown_5.1.0.css"));
            html = $$"""
                <head>
                <base target="_blank">
                <meta name="color-scheme" content="light dark">
                <style>
                body::-webkit-scrollbar {display: none;}
                {{css}}
                </style>
                </head>
                <body style="background-color: transparent;">
                <article class="markdown-body" style="background-color: transparent;">
                <h1>{{release.Name}}</h1>
                <blockquote>发布于 {{release.PublishedAt:yyyy-MM-dd HH:mm:ss}}</blockquote>
                {{html}}
                </article>
                </body>
                """;
            await webview.EnsureCoreWebView2Async();
            webview.CoreWebView2.Settings.AreDevToolsEnabled = false;
            webview.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            webview.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
            webview.NavigateToString(html);
            AppSetting.TrySetValue(SettingKeys.ShowUpdateContentOnLoaded, false);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "更新内容");
            NotificationProvider.Error(ex, "更新内容");
        }
    }


    [RelayCommand]
    private async Task OpenReleaseHistoryPageAsync()
    {
        await Launcher.LaunchUriAsync(new("https://github.com/xunkong/xunkong/releases"));
    }


}
