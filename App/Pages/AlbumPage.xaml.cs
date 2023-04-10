using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
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
        await Task.Delay(100);
        await vm.InitializeDataAsync();
    }



    private async void _Button_CopyImage_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            if (button.DataContext is FileInfo fileInfo)
            {
                try
                {
                    OperationHistory.AddToDatabase("CopyAlbumImage");
                    Logger.TrackEvent("CopyAlbumImage");
                    var file = await StorageFile.GetFileFromPathAsync(fileInfo.FullName);
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
        if (e.ClickedItem is FileInfo file)
        {
            var list = vm.ImageList.Select(x => WallpaperInfoEx.FromUri(x.FullName)).ToList();
            var current = list.FirstOrDefault(x => x.Url == file.FullName);
            if (current != null)
            {
                MainWindow.Current.SetFullWindowContent(new ImageViewer { CurrentImage = current, ImageCollection = list });
            }
        }
    }






}
