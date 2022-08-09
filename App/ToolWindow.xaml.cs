using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Vanara.PInvoke;
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


    public ToolWindow()
    {
        Current = this;
        this.InitializeComponent();
        InitializeWindow();
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
        _backdrop = new SystemBackdropHelper(this, BackbdropFallBackBehavior.None);
        if (_backdrop.TrySetBackdrop())
        {
            Grid_Root.Background = null;
        }
        HWND = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(HWND);
        AppWindow = AppWindow.GetFromWindowId(windowId);
        Title = XunkongEnvironment.AppName;
        TitleName.Text = XunkongEnvironment.AppName;
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
    }


    private void HandleActivationEvent(AppActivationArguments e)
    {
        if (e.Kind == ExtendedActivationKind.Protocol || e.Kind == ExtendedActivationKind.ProtocolForResults)
        {
            if (e.Data is IProtocolActivatedEventArgs args)
            {
                if (args.Uri.Host == "get-cookie")
                {
                    Frame_Root.Content = new GetCookiePage(args);
                }
            }
        }
    }



}
