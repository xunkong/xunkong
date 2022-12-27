using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Composition;
using Microsoft.Graphics.DirectX;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.Windows.AppLifecycle;
using System.Numerics;
using System.Runtime.InteropServices;
using Vanara.PInvoke;
using Windows.ApplicationModel;
using Windows.Graphics;
using WinRT.Interop;
using Xunkong.Desktop.Pages;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{

    public IntPtr HWND { get; private set; }

    private AppWindow _appWindow;

    private SystemBackdropHelper _backdrop;

    public static new MainWindow Current { get; private set; }

    public int Height => _appWindow.Size.Height;

    public int Width => _appWindow.Size.Width;

    public double UIScale => (double)User32.GetDpiForWindow(HWND) / 96;

    public XamlRoot XamlRoot => Content.XamlRoot;

    public DisplayArea DisplayArea => DisplayArea.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(HWND), DisplayAreaFallback.Primary);

    public ElementTheme ActualTheme => ((FrameworkElement)Content).ActualTheme;


    public MainWindow()
    {
        Current = this;
        this.InitializeComponent();
        InitializeWindow();
#if !DEBUG
        AppSetting.TryGetValue<bool>(SettingKeys.HasShownWelcomePage, out var shown);
        if (shown)
        {
#endif
        RootFrame.Content = new MainPage();
#if !DEBUG
        }
        else
        {
            RootFrame.Content = new WelcomPage();
        }
#endif
    }


    public MainWindow(AppActivationArguments args)
    {
        Current = this;
        this.InitializeComponent();
        InitializeWindow();
    }


    private void InitializeWindow()
    {
        InitializeBackdrop();
        InitializeWindowState();
        NotificationProvider.Initialize(InfoBarContainer);
        Closed += MainWindow_Closed;
    }


    private void InitializeBackdrop()
    {
        _backdrop = new SystemBackdropHelper(this);
        if (_backdrop.TrySetBackdrop())
        {
            windowBackground.Visibility = Visibility.Collapsed;
        }
    }


    private void InitializeWindowState()
    {
        HWND = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(HWND);
        _appWindow = AppWindow.GetFromWindowId(windowId);
        _appWindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, "Assets/Logos/logo.ico"));
        this.Title = XunkongEnvironment.AppName;
        if (AppWindowTitleBar.IsCustomizationSupported())
        {
            var scale = this.UIScale;
            var top = (int)(48 * scale);
            var titleBar = _appWindow.TitleBar;
            titleBar.ExtendsContentIntoTitleBar = true;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            titleBar.SetDragRectangles(new RectInt32[] { new RectInt32(top, 0, 10000, top) });
            // 解决 Windows 10 上标题栏无法拖动的问题
            // https://github.com/microsoft/WindowsAppSDK/issues/2976
            _appWindow.ResizeClient(_appWindow.ClientSize);
        }
        else
        {
            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(AppTitleBar);
        }
        if (AppSetting.GetValue<bool>(SettingKeys.IsMainWindowMaximum))
        {
            User32.ShowWindow(HWND, ShowWindowCommand.SW_MAXIMIZE);
            return;
        }
        if (AppSetting.TryGetValue<ulong>(SettingKeys.MainWindowRect, out var value))
        {
            var display = DisplayArea;
            var workAreaWidth = display.WorkArea.Width;
            var workAreaHeight = display.WorkArea.Height;
            var rect = new WindowRect(value);
            if (rect.Left > 0 && rect.Top > 0 && rect.Right < workAreaWidth && rect.Bottom < workAreaHeight)
            {
                _appWindow.MoveAndResize(rect.ToRectInt32());
            }
        }
    }




    private void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        SaveWindowState();
        BackupService.AutoBackupDatabase();
    }


    /// <summary>
    /// 保存窗口状态
    /// </summary>
    private void SaveWindowState()
    {
        var wpl = new User32.WINDOWPLACEMENT();
        if (User32.GetWindowPlacement(HWND, ref wpl))
        {
            AppSetting.SetValue(SettingKeys.IsMainWindowMaximum, wpl.showCmd == ShowWindowCommand.SW_MAXIMIZE);
            var p = _appWindow.Position;
            var s = _appWindow.Size;
            var rect = new WindowRect(p.X, p.Y, s.Width, s.Height);
            AppSetting.SetValue(SettingKeys.MainWindowRect, rect.Value);
        }
    }



    /// <summary>
    /// 切换窗口主题
    /// </summary>
    /// <param name="theme"></param>
    /// <param name="center"></param>
    public async void ChangeApplicationTheme(int theme, Vector2? center = null)
    {
        try
        {
            var compositor = ElementCompositionPreview.GetElementVisual(Content).Compositor;
            var graphicsDevice = CanvasComposition.CreateCompositionGraphicsDevice(compositor, CanvasDevice.GetSharedDevice());

            var windowSize = Content.ActualSize;

            // 捕获主题修改前的图面，此图面不随可视化树的变化而变化
            var oldSurface = await graphicsDevice.CaptureAsync(ElementCompositionPreview.GetElementVisual(Content),
                                                               new SizeInt32((int)Content.ActualSize.X, (int)Content.ActualSize.Y),
                                                               DirectXPixelFormat.B8G8R8A8UIntNormalized,
                                                               DirectXAlphaMode.Premultiplied,
                                                               0);
            var oldTheme = RootBorder.ActualTheme;
            RootBorder.RequestedTheme = theme switch
            {
                1 => ElementTheme.Light,
                2 => ElementTheme.Dark,
                _ => ElementTheme.Default,
            };

            var max = Math.Max(_appWindow.ClientSize.Width, _appWindow.ClientSize.Height);

            // 计算动画的中心位置
            using var ellipse = compositor.CreateEllipseGeometry();
            using var clip = compositor.CreateGeometricClip(ellipse);
            if (center != null)
            {
                clip.Offset = center.Value;
            }

            if (RootBorder.ActualTheme == oldTheme || windowBackground.Visibility == Visibility.Collapsed)
            {
                return;
            }

            if (RootBorder.ActualTheme is ElementTheme.Light)
            {
                // 新主题是深色模式

                // 主题修改后的图面，此图面与可视化树同步变化
                using var newSurface = compositor.CreateVisualSurface();
                newSurface.SourceVisual = ElementCompositionPreview.GetElementVisual(RootGrid);
                newSurface.SourceSize = windowSize;

                using var oldSprite = compositor.CreateSpriteVisual();
                oldSprite.Brush = compositor.CreateSurfaceBrush(oldSurface);
                oldSprite.Size = windowSize;
                oldSprite.IsHitTestVisible = false;

                using var newSprite = compositor.CreateSpriteVisual();
                newSprite.Brush = compositor.CreateSurfaceBrush(newSurface);
                newSprite.Size = windowSize;
                newSprite.Clip = clip;
                newSprite.IsHitTestVisible = false;

                using var container = compositor.CreateContainerVisual();
                container.Size = windowSize;
                container.Children.InsertAtTop(oldSprite);
                container.Children.InsertAtTop(newSprite);
                ElementCompositionPreview.SetElementChildVisual(Content, container);

                // 从小到大
                using var animation = compositor.CreateVector2KeyFrameAnimation();
                animation.InsertKeyFrame(0, Vector2.Zero);
                animation.InsertKeyFrame(1, new Vector2(max));
                animation.Duration = TimeSpan.FromMilliseconds(1000);
                ellipse.StartAnimation("Radius", animation);

                await Task.Delay(400);
            }
            else
            {
                // 新主题是浅色模式

                using var oldSprite = compositor.CreateSpriteVisual();
                oldSprite.Brush = compositor.CreateSurfaceBrush(oldSurface);
                oldSprite.Size = windowSize;
                oldSprite.Clip = clip;
                oldSprite.IsHitTestVisible = false;
                ElementCompositionPreview.SetElementChildVisual(Content, oldSprite);

                // 从大到小
                using var animation = compositor.CreateVector2KeyFrameAnimation();
                animation.InsertKeyFrame(0, new Vector2(max));
                animation.InsertKeyFrame(1, Vector2.Zero);
                animation.Duration = TimeSpan.FromMilliseconds(400);
                ellipse.StartAnimation("Radius", animation);

                await Task.Delay(400);
            }
        }
        catch { }
    }



    /// <summary>
    /// 窗口 Root Frame 导航
    /// </summary>
    /// <param name="sourcePageType"></param>
    /// <param name="param"></param>
    /// <param name="infoOverride"></param>
    public void Navigate(Type sourcePageType, object? param = null, NavigationTransitionInfo? infoOverride = null)
    {
        if (param is null)
        {
            RootFrame.Navigate(sourcePageType);
        }
        else if (infoOverride is null)
        {
            RootFrame.Navigate(sourcePageType, param);
        }
        else
        {
            RootFrame.Navigate(sourcePageType, param, infoOverride);
        }
    }


    /// <summary>
    /// 设置覆盖于窗口上的内容
    /// </summary>
    /// <param name="content"></param>
    public void SetFullWindowContent(Control content)
    {
        FullWindowContent.Visibility = Visibility.Visible;
        FullWindowContent.Content = content;
    }


    /// <summary>
    /// 清除覆盖于窗口上的内容
    /// </summary>
    public void CloseFullWindowContent()
    {
        FullWindowContent.Content = null;
        FullWindowContent.Visibility = Visibility.Collapsed;
    }


    public void MoveToTop()
    {
        User32.ShowWindow(HWND, ShowWindowCommand.SW_SHOWDEFAULT);
        User32.SetForegroundWindow(HWND);
    }


    public bool TryChangeBackdrop(uint value)
    {
        var result = _backdrop.TryChangeBackdrop(value, out _);
        if (result && (value & 0xF) > 0)
        {
            windowBackground.Visibility = Visibility.Collapsed;
        }
        else
        {
            windowBackground.Visibility = Visibility.Visible;
        }
        return result;
    }



    [StructLayout(LayoutKind.Explicit)]
    private struct WindowRect
    {
        [FieldOffset(0)]
        public short X;

        [FieldOffset(2)]
        public short Y;

        [FieldOffset(4)]
        public short Width;

        [FieldOffset(6)]
        public short Height;

        [FieldOffset(0)]
        public ulong Value;

        public int Left => X;

        public int Top => Y;

        public int Right => X + Width;

        public int Bottom => Y + Height;

        public WindowRect(int x, int y, int width, int height)
        {
            Value = 0;
            X = (short)x;
            Y = (short)y;
            Width = (short)width;
            Height = (short)height;
        }

        public WindowRect(ulong value)
        {
            X = 0;
            Y = 0;
            Width = 0;
            Height = 0;
            Value = value;
        }

        public RectInt32 ToRectInt32()
        {
            return new RectInt32(X, Y, Width, Height);
        }

    }




}
