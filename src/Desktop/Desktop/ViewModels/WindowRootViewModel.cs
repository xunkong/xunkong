using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Xunkong.Core.Hoyolab;
using Xunkong.Desktop.Services;
using System.Net;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Xunkong.Core.XunkongApi;

namespace Xunkong.Desktop.ViewModels
{

    [InjectService]
    internal partial class WindowRootViewModel : ObservableObject
    {

        private readonly ILogger<WindowRootViewModel> _logger;

        private readonly IDbContextFactory<XunkongDbContext> _dbFactory;

        private readonly UserSettingService _userSettingService;

        private readonly HoyolabService _hoyolabService;

        private readonly XunkongApiService _xunkongApiService;



        public WindowRootViewModel(ILogger<WindowRootViewModel> logger, IDbContextFactory<XunkongDbContext> dbFactory, UserSettingService userSettingService, HoyolabService hoyolabService, XunkongApiService xunkongApiService)
        {
            _logger = logger;
            _dbFactory = dbFactory;
            _userSettingService = userSettingService;
            _hoyolabService = hoyolabService;
            _xunkongApiService = xunkongApiService;
        }



        public void CheckWebView2Runtime()
        {
            try
            {
                _ = Microsoft.Web.WebView2.Core.CoreWebView2Environment.GetAvailableBrowserVersionString();
            }
            catch
            {
                const string url = "https://go.microsoft.com/fwlink/p/?LinkId=2124703";
                var button = new Button { Content = "下载", HorizontalAlignment = HorizontalAlignment.Right };
                button.Click += (_, _) => Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true,
                });
                var infoBar = new InfoBar
                {
                    Severity = InfoBarSeverity.Warning,
                    Title = "警告",
                    Message = "没有找到WebView2运行时，会影响软件必要的功能。",
                    ActionButton = button,
                    IsOpen = true,
                };
                InfoBarHelper.Show(infoBar);
            }
        }




        public async void CheckVersionUpdateAsync()
        {
            if (XunkongEnvironment.Channel == ChannelType.Development)
            {
                try
                {
                    var version = await _xunkongApiService.CheckUpdateAsync(ChannelType.Development);
                    if (version.Version > XunkongEnvironment.AppVersion && !string.IsNullOrWhiteSpace(version.PackageUrl))
                    {
                        var button = new Button { Content = "下载", HorizontalAlignment = HorizontalAlignment.Right };
                        button.Click += (_, _) => Process.Start(new ProcessStartInfo
                        {
                            FileName = version.PackageUrl,
                            UseShellExecute = true,
                        });
                        var infoBar = new InfoBar
                        {
                            Severity = InfoBarSeverity.Success,
                            Title = $"新版本 {version.Version}",
                            Message = version.Abstract,
                            ActionButton = button,
                            IsOpen = true,
                        };
                        InfoBarHelper.Show(infoBar);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error when check version update startup in {MethodName}", nameof(CheckVersionUpdateAsync));
                }
            }
        }



    }
}
