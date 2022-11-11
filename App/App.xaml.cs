using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using System.Web;
using Windows.ApplicationModel.Activation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{

    private MainWindow mainWindow;

    private ToolWindow toolWindow;


    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
        InitializeApplicationTheme();
        UnhandledException += App_UnhandledException;
    }


    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        Logger.Error(e.Exception, "崩溃了");
        Logger.CloseAndFlush();
    }


    private void InitializeApplicationTheme()
    {
        if (AppSetting.TryGetValue<int>(SettingKeys.ApplicationTheme, out var themeIndex))
        {
            if (themeIndex == 1)
            {
                RequestedTheme = ApplicationTheme.Light;
            }
            if (themeIndex == 2)
            {
                RequestedTheme = ApplicationTheme.Dark;
            }
        }
    }


    /// <summary>
    /// Invoked when the application is launched normally by the end user.  Other entry points
    /// will be used such as when the application is launched to open a specific file.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs _)
    {
        var a = Environment.GetCommandLineArgs();
        var args = AppInstance.GetCurrent().GetActivatedEventArgs();
        var firstInstance = AppInstance.FindOrRegisterForKey("Main");
        if (!firstInstance.IsCurrent && args.Kind == ExtendedActivationKind.Launch)
        {
            // 已存在主窗口 & 激活方式为 Launch，重定向到主窗口
            await firstInstance.RedirectActivationToAsync(args);
            Environment.Exit(0);
        }
        else
        {
            HandleActivationEvent(args, firstInstance);
        }
    }


    private void OnActivated(object? sender, AppActivationArguments e)
    {
        HandleActivationEvent(e);
    }


    private async void HandleActivationEvent(AppActivationArguments e, AppInstance? firstInstance = null)
    {
        if (firstInstance is null)
        {
            // 重定向后
            if (mainWindow is null)
            {
                Environment.Exit(0);
            }
            else
            {
                mainWindow.DispatcherQueue.TryEnqueue(mainWindow.MoveToTop);
            }
            if (e.Kind == ExtendedActivationKind.Protocol)
            {
                // 协议启动
                if (e.Data is IProtocolActivatedEventArgs args)
                {
                    if (args.Uri.Host.ToLower() is "post-message")
                    {
                        try
                        {
                            var id = args.Uri.AbsolutePath.Replace("/", "");
                            var query = HttpUtility.ParseQueryString(args.Uri.Query);
                            var message = new ProtocolMessage { Message = id };
                            foreach (var key in query.AllKeys)
                            {
                                message.Data.Add(key!, query[key]!);
                            }
                            mainWindow.DispatcherQueue.TryEnqueue(() => WeakReferenceMessenger.Default.Send(message));
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, "处理重定向后 post-massage 协议");
                        }
                    }
                }
            }
            if (e.Kind == ExtendedActivationKind.ToastNotification)
            {
                if (e.Data is IToastNotificationActivatedEventArgs args)
                {
                    if (args.Argument == "CancelPreCacheAllFiles")
                    {
                        // 停止下载图片
                        PreCacheService.StopPreCache();
                    }
                }
            }
        }
        else
        {
            // 重定向前, firstInstance.Key 一定是 Main
            if (e.Kind == ExtendedActivationKind.Launch)
            {
                // firstInstance.IsCurrent ，若不是，之前已经重定向过了
                firstInstance.Activated += OnActivated;
                mainWindow = new MainWindow();
                mainWindow.Activate();

            }
            else
            {
                bool handled = false;
                if (firstInstance.IsCurrent)
                {
                    firstInstance.UnregisterKey();
                }
                if (e.Kind == ExtendedActivationKind.Protocol)
                {
                    // 协议启动
                    if (e.Data is IProtocolActivatedEventArgs args)
                    {
                        if (args.Uri.Host.ToLower() is "get-cookie" or "import-achievement" or "settings")
                        {
                            toolWindow = new ToolWindow(e);
                            toolWindow.Activate();
                            handled = true;
                        }
                        if (args.Uri.Host.ToLower() is "post-message")
                        {
                            await firstInstance.RedirectActivationToAsync(e);
                            // 直接退出，如果没窗口，还发啥消息
                            Environment.Exit(0);
                        }
                    }
                }
                if (e.Kind == ExtendedActivationKind.ToastNotification)
                {
                    if (e.Data is IToastNotificationActivatedEventArgs args)
                    {
                        if (!firstInstance.IsCurrent)
                        {
                            await firstInstance.RedirectActivationToAsync(e);
                        }
                        Environment.Exit(0);
                    }
                }
                if (handled)
                {
                    return;
                }
                else
                {
                    // 协议参数解析失败
                    toolWindow = new ToolWindow();
                    toolWindow.Activate();
                }
            }
        }
    }




}
