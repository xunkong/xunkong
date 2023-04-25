using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Windows.System;
using Xunkong.Desktop.Controls;
using Xunkong.Desktop.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class AlbumPage : Page
{

    private AlbumViewModel vm => (DataContext as AlbumViewModel)!;


    public AlbumPage()
    {
        this.InitializeComponent();
        DataContext = ServiceProvider.GetService<AlbumViewModel>();
        Loaded += AlbumPage_Loaded;
    }



    private async void AlbumPage_Loaded(object sender, RoutedEventArgs e)
    {
        await Task.Delay(30);
        vm.InitializeImageFolder();
    }



    private async void _Button_CopyImage_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            if (button.DataContext is AlbumPage_ImageItem item)
            {
                try
                {
                    OperationHistory.AddToDatabase("CopyAlbumImage");
                    Logger.TrackEvent("CopyAlbumImage");
                    var file = await StorageFile.GetFileFromPathAsync(item.FullName);
                    ClipboardHelper.SetBitmap(file);
                    button.Content = new SymbolIcon(Symbol.Accept);
                    await Task.Delay(3000);
                    button.Content = new SymbolIcon(Symbol.Copy);
                }
                catch (Exception ex)
                {
                    NotificationProvider.Error(ex, "复制图片");
                    Logger.Error(ex, "复制图片");
                }
            }
        }
    }



    private void _GridView_Images_ItemClick(object sender, ItemClickEventArgs e)
    {
        try
        {
            if (e.ClickedItem is AlbumPage_ImageItem item)
            {
                var list = vm.FolderList[Pivot_ImageFolder.SelectedIndex].ImageList.Select(x => WallpaperInfoEx.FromUri(x.FullName)).ToList();
                var current = list.FirstOrDefault(x => x.Url == item.FullName);
                if (current != null)
                {
                    MainWindow.Current.SetFullWindowContent(new ImageViewer { CurrentImage = current, ImageCollection = list });
                }
            }
        }
        catch (Exception ex)
        {

        }
    }



    [RelayCommand]
    private async Task OpenImageFolderAsync(string folderIndex)
    {
        try
        {
            if (folderIndex == "0")
            {
                if (AppSetting.TryGetValue<string>(SettingKeys.ScreenFolderPath, out var folder))
                {
                    folder = Path.GetFullPath(folder!);
                    await Launcher.LaunchFolderPathAsync(folder);
                }
            }
            if (folderIndex == "1")
            {
                var folder = AppSetting.GetValue<string>(SettingKeys.GameScreenshotBackupFolder) ?? Path.Combine(XunkongEnvironment.UserDataPath, "Screenshot");
                folder = Path.GetFullPath(folder);
                await Launcher.LaunchFolderPathAsync(folder);
            }
            if (folderIndex == "2")
            {
                var folder = AppSetting.GetValue<string>(SettingKeys.WallpaperSaveFolder) ?? Path.Combine(XunkongEnvironment.UserDataPath, "Wallpaper");
                folder = Path.GetFullPath(folder);
                await Launcher.LaunchFolderPathAsync(folder);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }



    [RelayCommand]
    private async Task SetImageFolderAsync(string folderIndex)
    {
        try
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, MainWindow.Current.HWND);
            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder is not null)
            {
                string folderName = "";
                if (folderIndex == "0")
                {
                    AppSetting.SetValue(SettingKeys.ScreenFolderPath, folder.Path);
                    folderName = "游戏截图";
                }
                if (folderIndex == "1")
                {
                    AppSetting.SetValue(SettingKeys.GameScreenshotBackupFolder, folder.Path);
                    folderName = "截图备份";
                }
                if (folderIndex == "2")
                {
                    AppSetting.SetValue(SettingKeys.WallpaperSaveFolder, folder.Path);
                    folderName = "壁纸收藏";
                }
                if (vm.FolderList.FirstOrDefault(x => x.Header == folderName) is AlbumPage_ImageFoler item)
                {
                    item.Dispose();
                    vm.FolderList.Remove(item);
                }
                vm.InitializeImageFolder();
            }
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex, "设置截图文件夹");
            Logger.Error(ex, "设置截图文件夹");
        }
    }




}
