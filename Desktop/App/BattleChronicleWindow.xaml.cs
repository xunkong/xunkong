using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Vanara.PInvoke;
using Windows.Graphics;
using Xunkong.Hoyolab.Account;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class BattleChronicleWindow : Window
{



    private readonly HoyolabService _hoyolabService;


    public BattleChronicleWindow()
    {
        this.InitializeComponent();
        this.AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
        CenterInScreen();
        _hoyolabService = ServiceProvider.GetService<HoyolabService>()!;
        bridge.Loaded += BattleChroniclePage_Loaded;
    }


    [ObservableProperty]
    private bool allRole;


    [ObservableProperty]
    private GenshinRoleInfo genshinRole;



    private void BattleChroniclePage_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            User32.SetForegroundWindow((nint)AppWindow.Id.Value);
            var role = _hoyolabService.GetLastSelectedOrFirstGenshinRoleInfo();
            if (role is null)
            {
                return;
            }
            GenshinRole = role;
        }
        catch (Exception ex)
        {

        }
    }



    public double UIScale => (double)User32.GetDpiForWindow((nint)AppWindow.Id.Value) / 96;

    public XamlRoot XamlRoot => Content.XamlRoot;



    private void CenterInScreen()
    {
        RectInt32 workArea = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Primary).WorkArea;
        int h = (int)(workArea.Height * 0.95);
        int w = (int)(h * 9.0 / 16.0);
        if (w > workArea.Width)
        {
            w = (int)(workArea.Width * 0.95);
            h = (int)(w * 16.0 / 9.0);
        }
        int x = workArea.X + (workArea.Width - w) / 2;
        int y = workArea.Y + (workArea.Height - h) / 2;
        AppWindow.MoveAndResize(new RectInt32(x, y, w, h));
    }





}
