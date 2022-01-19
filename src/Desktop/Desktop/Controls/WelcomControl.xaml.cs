using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json.Nodes;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using WinRT;
using Microsoft.Windows;
using Microsoft.Win32;
using WinRT.Interop;
using Vanara.PInvoke;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Controls
{
    public sealed partial class WelcomControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private readonly string url = "https://sdk-static.mihoyo.com/hk4e_cn/mdk/launcher/api/content?filter_adv=true&language=zh-cn&launcher_id=18&key=eYd89JmJ";

        public UIElement AppTitleBar => _appTitleBar;

        public Action FinishSetting { get; set; }

        public WelcomControl()
        {
            this.InitializeComponent();
            Loading += WelcomControl_Loading;
        }


        private string _UserDataPath;
        public string UserDataPath
        {
            get { return _UserDataPath; }
            set
            {
                _UserDataPath = value;
                OnPropertyChanged();
            }
        }


        private string _WebView2Status;
        public string WebView2Status
        {
            get { return _WebView2Status; }
            set
            {
                _WebView2Status = value;
                OnPropertyChanged();
            }
        }




        private async void WelcomControl_Loading(FrameworkElement sender, object args)
        {
            try
            {
                var version = Microsoft.Web.WebView2.Core.CoreWebView2Environment.GetAvailableBrowserVersionString();
                WebView2Status = $"已安装 ({version})";
            }
            catch
            {
                WebView2Status = "未检测到 WebView2 Runtime";
            }
            using var client = new HttpClient();
            try
            {
                var str = await client.GetStringAsync(url);
                var node = JsonNode.Parse(str);
                var i = (string)node["data"]["adv"]["background"];
                Debug.WriteLine(i);
                _ImageEx_Background.Source = i;
            }
            catch (Exception ex)
            {

            }
        }



        private void _Button_Close_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Exit();
        }

        private async void _Button_SelectFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new FolderPicker();
                dialog.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                dialog.FileTypeFilter.Add("*");
                InitializeWithWindow.Initialize(dialog, MainWindow.Hwnd);
                var folder = await dialog.PickSingleFolderAsync();
                if (folder != null)
                {
                    CreateUserDataFolder(folder.Path);
                }
            }
            catch (Exception ex)
            {
                InfoBarHelper.Error(ex.Message);
            }
        }



        private void _Button_Download_Click(object sender, RoutedEventArgs e)
        {
            const string url = "https://go.microsoft.com/fwlink/p/?LinkId=2124703";
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = url,
            };
            Process.Start(startInfo);
        }

        private void _Button_RefreshStats_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var version = Microsoft.Web.WebView2.Core.CoreWebView2Environment.GetAvailableBrowserVersionString();
                WebView2Status = $"已安装 ({version})";
            }
            catch
            {
                WebView2Status = "未检测到 WebView2 Runtime";
            }
        }

        private async void _Button_Finish_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(UserDataPath))
                {
                    var defaultFolder = Path.Combine(ApplicationData.Current.LocalCacheFolder.Path, "Local\\Xunkong");
                    Directory.CreateDirectory(defaultFolder);
                    await File.WriteAllTextAsync(Path.Combine(defaultFolder, "XunkongRoot"), "This folder is used by Xunkong Desktop.");
                    ApplicationData.Current.LocalSettings.Values[SettingKeys.UserDataPath] = defaultFolder;
                    UserDataPath = defaultFolder;
                }
                FinishSetting();
            }
            catch (Exception ex)
            {
                InfoBarHelper.Error(ex.Message);
            }
        }






        private async void CreateUserDataFolder(string path)
        {
            // 存在根文件
            if (File.Exists(Path.Combine(path, "XunkongRoot")))
            {
                // 就是它了
                ApplicationData.Current.LocalSettings.Values[SettingKeys.UserDataPath] = path;
                UserDataPath = path;
            }
            // 不存在根文件
            else
            {
                // 有其他文件
                if (Directory.GetFiles(path).Any())
                {
                    // 创建子文件夹
                    Directory.CreateDirectory(Path.Combine(path, "Xunkong"));
                    await File.WriteAllTextAsync(Path.Combine(path, "Xunkong/XunkongRoot"), "This folder is used by Xunkong Desktop.");
                    ApplicationData.Current.LocalSettings.Values[SettingKeys.UserDataPath] = Path.Combine(path, "Xunkong");
                    UserDataPath = Path.Combine(path, "Xunkong");
                }
                // 没有其他文件
                else
                {
                    // 就是它了
                    await File.WriteAllTextAsync(Path.Combine(path, "XunkongRoot"), "This folder is used by Xunkong Desktop.");
                    ApplicationData.Current.LocalSettings.Values[SettingKeys.UserDataPath] = path;
                    UserDataPath = path;
                }
            }
        }

        private void _ImageEx_Background_ImageExOpened(object sender, CommunityToolkit.WinUI.UI.Controls.ImageExOpenedEventArgs e)
        {

        }

        private void _ImageEx_Background_ImageExFailed(object sender, CommunityToolkit.WinUI.UI.Controls.ImageExFailedEventArgs e)
        {

        }
    }
}
