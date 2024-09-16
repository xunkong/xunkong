using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Vanara.PInvoke;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using WinRT.Interop;
using Xunkong.Desktop.Pages;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ToolWindow : Window
{

    public IntPtr HWND { get; private set; }

    public AppWindow AppWindow { get; private set; }

    private SystemBackdropHelper _backdrop;

    public static new ToolWindow Current { get; private set; }

    public int Height => AppWindow.Size.Height;

    public int Width => AppWindow.Size.Width;

    public double UIScale => (double)User32.GetDpiForWindow(HWND) / 96;

    public XamlRoot XamlRoot => Content.XamlRoot;

    public DisplayArea DisplayArea => DisplayArea.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(HWND), DisplayAreaFallback.Primary);


    /// <summary>
    /// 参数解析失败
    /// </summary>
    public ToolWindow()
    {
        Current = this;
        this.InitializeComponent();
        InitializeWindow();
        ResizeToCenter(360, 480);
        c_StackPanel_Error.Visibility = Visibility.Visible;
    }

    public ToolWindow(AppActivationArguments e)
    {
        Current = this;
        this.InitializeComponent();
        InitializeWindow();
        HandleActivationEvent(e);
    }


    private void InitializeWindow()
    {
        _backdrop = new SystemBackdropHelper(this);
        if (_backdrop.TrySetBackdrop())
        {
            Grid_Root.Background = null;
        }
        HWND = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(HWND);
        AppWindow = AppWindow.GetFromWindowId(windowId);
        AppWindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, "Assets/Logos/logo.ico"));
        Title = XunkongEnvironment.AppName;
        TitleName.Text = XunkongEnvironment.AppName;
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
    }


    private void HandleActivationEvent(AppActivationArguments e)
    {
        bool handled = false;
        if (e.Kind == ExtendedActivationKind.Protocol || e.Kind == ExtendedActivationKind.ProtocolForResults)
        {
            if (e.Data is IProtocolActivatedEventArgs args)
            {
                if (args.Uri.Host.ToLower() == "get-cookie")
                {
                    Frame_Root.Content = new GetCookiePage(args);
                    handled = true;
                }
                if (args.Uri.Host.ToLower() == "import-achievement")
                {
                    Frame_Root.Content = new ImportAchievementPage(args);
                    handled = true;
                }
                if (args.Uri.Host.ToLower() == "settings")
                {
                    Frame_Root.Content = new SettingToolPage(args);
                    handled = true;
                }
            }
        }
        if (e.Kind == ExtendedActivationKind.ToastNotification)
        {
            if (e.Data is IToastNotificationActivatedEventArgs args)
            {
                if (args.Argument.StartsWith("DailyNoteTask_VerifyAccount"))
                {
                    Frame_Root.Content = new VerifyAccountPage(args);
                    handled = true;
                }
            }
        }
        if (!handled)
        {
            ResizeToCenter(360, 480);
            c_StackPanel_Error.Visibility = Visibility.Visible;
        }
    }





    public void ResizeToCenter(int width, int height)
    {
        var uiScale = UIScale;
        var scaledwidth = (int)(width * uiScale);
        var scaledheight = (int)(height * uiScale);
        var workArea = DisplayArea.WorkArea;
        var left = (workArea.Width - scaledwidth) / 2;
        var top = (workArea.Height - scaledheight) / 2;
        AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(left, top, scaledwidth, scaledheight));
    }





}
