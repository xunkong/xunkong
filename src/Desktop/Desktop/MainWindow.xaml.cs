using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Vanara.PInvoke;
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


        public Func<IServiceProvider> ConfigureServices { get; set; }


        public static new Window Current { get; private set; }

        public static XamlRoot XamlRoot => Current.Content.XamlRoot;

        public static IntPtr Hwnd { get; private set; }

        public MainWindow(bool showWelcom = false)
        {
            Current = this;
            this.InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            InfoBarHelper.Initialize(_InfoBarContainer);
            Hwnd = WindowNative.GetWindowHandle(this);
            if (showWelcom)
            {
                var dpi = User32.GetDpiForWindow(Hwnd);
                float scale = dpi / 72;
                var XLength = User32.GetSystemMetrics(User32.SystemMetric.SM_CXSCREEN);
                var YLength = User32.GetSystemMetrics(User32.SystemMetric.SM_CYSCREEN);
                var width = (int)(960 * scale);
                var heigh = (int)(540 * scale);
                var x = (XLength - width) / 2;
                var y = (YLength - heigh) / 2;
                User32.SetWindowPos(Hwnd, HWND.NULL, x, y, width, heigh, 0);
                var welcomControl = new WelcomControl();
                welcomControl.FinishSetting = FinishSetting;
                _rootContent.Content = welcomControl;
                SetTitleBar(welcomControl.AppTitleBar);
            }
            else
            {
                var rootControl = new MainWindowView();
                _rootContent.Content = rootControl;
                SetTitleBar(rootControl.AppTitleBar);
            }
        }



        private void FinishSetting()
        {
            App.Current.Services = ConfigureServices();
            var rootControl = new MainWindowView();
            _rootContent.Content = rootControl;
            SetTitleBar(rootControl.AppTitleBar);
        }




    }
}
