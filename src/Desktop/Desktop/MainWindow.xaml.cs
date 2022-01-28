using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Vanara.PInvoke;
using Windows.Storage;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



        public static new Window Current { get; private set; }

        public static XamlRoot XamlRoot => Current.Content.XamlRoot;

        public static IntPtr Hwnd { get; private set; }

        public MainWindow(bool showWelcom = false)
        {
            Current = this;
            Closed += MainWindow_Closed;
            this.InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            InfoBarHelper.Initialize(_InfoBarContainer);
            WeakReferenceMessenger.Default.Register<FinishWelcomSettingMessage>(this, (_, _) => FinishSettingAndChangeContent());
            Hwnd = WindowNative.GetWindowHandle(this);
            InitializeWindowSize();
            if (showWelcom)
            {
                var welcomControl = new WelcomControl();
                _rootContent.Content = welcomControl;
                SetTitleBar(welcomControl.AppTitleBar);
            }
            else
            {
                var rootControl = new WindowRootView();
                _rootContent.Content = rootControl;
                SetTitleBar(rootControl.AppTitleBar);
            }
        }



        private void InitializeWindowSize()
        {
            var setting = ApplicationData.Current.LocalSettings.Values;
            // 无法在启动时最大化窗口
            //var showCmd = (bool)setting[SettingKeys.IsWindowMax] ? ShowWindowCommand.SW_MAXIMIZE : ShowWindowCommand.SW_NORMAL;
            var left = (int)(setting[SettingKeys.WindowLeft] ?? 0);
            var top = (int)(setting[SettingKeys.WindowTop] ?? 0);
            var right = (int)(setting[SettingKeys.WindowRight] ?? 0);
            var bottom = (int)(setting[SettingKeys.WindowBottom] ?? 0);
            var rect = new RECT(left, top, right, bottom);
            if (rect.Width * rect.Height == 0)
            {
                var dpi = User32.GetDpiForWindow(Hwnd);
                float scale = dpi / 72;
                var XLength = User32.GetSystemMetrics(User32.SystemMetric.SM_CXSCREEN);
                var YLength = User32.GetSystemMetrics(User32.SystemMetric.SM_CYSCREEN);
                var width = (int)(1280 * scale);
                var height = (int)(720 * scale);
                var x = (XLength - width) / 2;
                var y = (YLength - height) / 2;
                rect = new RECT(x, y, x + width, y + height);
            }
            var wp = new User32.WINDOWPLACEMENT
            {
                length = 44,
                showCmd = ShowWindowCommand.SW_NORMAL,
                ptMaxPosition = new System.Drawing.Point(-1, -1),
                rcNormalPosition = rect,
            };
            User32.SetWindowPlacement(Hwnd, ref wp);
        }





        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            var wp = new User32.WINDOWPLACEMENT();
            User32.GetWindowPlacement(Hwnd, ref wp);
            var setting = ApplicationData.Current.LocalSettings.Values;
            // 无法在启动时最大化窗口
            //setting[SettingKeys.IsWindowMax] = wp.showCmd == ShowWindowCommand.SW_SHOWMAXIMIZED;
            setting[SettingKeys.WindowLeft] = wp.rcNormalPosition.left;
            setting[SettingKeys.WindowTop] = wp.rcNormalPosition.top;
            setting[SettingKeys.WindowRight] = wp.rcNormalPosition.right;
            setting[SettingKeys.WindowBottom] = wp.rcNormalPosition.bottom;
        }

        private void FinishSettingAndChangeContent()
        {
            var rootControl = new WindowRootView();
            _rootContent.Content = rootControl;
            SetTitleBar(rootControl.AppTitleBar);
        }






    }
}
