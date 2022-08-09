using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
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
        bool handled = false;
        if (e.Kind != ExtendedActivationKind.Launch)
        {
            // 启动方式不为 Launch

            if (e.Kind == ExtendedActivationKind.Protocol || e.Kind == ExtendedActivationKind.ProtocolForResults)
            {
                // 协议启动
                if (e.Data is IProtocolActivatedEventArgs args)
                {
                    if (args.Uri.Host == "get-cookie")
                    {
                        toolWindow = new ToolWindow(e);
                        toolWindow.Activate();
                        handled = true;
                    }
                }
            }
        }

        if (handled)
        {
            // 已处理完协议
            if (firstInstance?.IsCurrent ?? false)
            {
                // 如果当前进程为第一个启动的进程，注销主窗口标记
                firstInstance?.UnregisterKey();
            }
        }
        else
        {
            // 未处理协议，或处理失败
            if (firstInstance == null)
            {
                // 通过 OnActived 激活，则当前为已重定向后的进程
                if (mainWindow is null)
                {
                    Environment.Exit(0);
                }
                else
                {
                    mainWindow.DispatcherQueue.TryEnqueue(mainWindow.SetForeground);
                }
            }
            else
            {
                // 通过 OnLaunch 激活，当前进程为重定向前的进程
                if (firstInstance.IsCurrent)
                {
                    // 是第一次打开的进程
                    firstInstance.Activated += OnActivated;
                    mainWindow = new MainWindow();
                    mainWindow.Activate();
                }
                else
                {
                    // 不是第一次打开的进程，重定向到第一次打开的进程
                    await firstInstance.RedirectActivationToAsync(e);
                    Environment.Exit(0);
                }
            }
        }
    }




}
