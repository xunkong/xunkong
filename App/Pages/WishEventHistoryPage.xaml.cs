using Microsoft.Graphics.Canvas;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI;
using WinRT.Interop;
using Xunkong.GenshinData.Character;
using Xunkong.GenshinData.Weapon;
using Xunkong.Hoyolab.Wishlog;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class WishEventHistoryPage : Page
{
    public WishEventHistoryPage()
    {
        this.InitializeComponent();
        NavigationCacheMode = AppSetting.GetValue<bool>(SettingKeys.EnableNavigationCache) ? NavigationCacheMode.Enabled : NavigationCacheMode.Disabled;
        Loaded += WishEventCDPage_Loaded;
    }


    [ObservableProperty]
    private List<WishEventHistoryPage_Column> columns;

    [ObservableProperty]
    private List<WishEventHistoryPage_LeftHeader> leftHeaders;

    [ObservableProperty]
    private List<WishEventHistoryPage_TopHeader> topHeaders;


    private void WishEventCDPage_Loaded(object sender, RoutedEventArgs e)
    {
        NotificationProvider.Information("提示", "鼠标左键拖动，鼠标右键打开菜单");
        _ComboBox.SelectedIndex = 0;
    }



    private async void _ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _ProgressRing.IsActive = true;
        await Task.Delay(100);
        try
        {
            if (_ComboBox.SelectedIndex == 0)
            {
                LoadCharacterEventData();
            }
            else
            {
                LoadWeaponEventData();
            }
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex, "加载卡池数据");
            Logger.Error(ex, "加载卡池数据");
        }
        finally
        {
            _ProgressRing.IsActive = false;
        }
    }


    private void LoadCharacterEventData()
    {
        using var litedb = DatabaseProvider.CreateLiteDB();
        var characters = litedb.GetCollection<CharacterInfo>().FindAll().ToList();
        var wishevents = litedb.GetCollection<WishEventInfo>().FindAll().ToList();
        var chaevents = wishevents.Where(x => x.QueryType == WishType.CharacterEvent).GroupBy(x => x.StartTime).ToList();
        var leftHeader = chaevents.Select(x => new WishEventHistoryPage_LeftHeader
        {
            Version = x.First().Version,
            StartTime = x.First().StartTime,
            EndTime = x.First().EndTime,
            UpItems = x.SelectMany(y => y.Rank5UpItems).Concat(x.SelectMany(y => y.Rank4UpItems).Distinct()).ToList()
        }).ToList();
        var columns = characters.Select(x => new WishEventHistoryPage_Column { Name = x.Name!, Icon = x.FaceIcon!, Rarity = x.Rarity, Element = x.Element }).ToList();
        foreach (var column in columns)
        {
            int index = 0;
            foreach (var header in leftHeader)
            {
                index++;
                if (header.UpItems.Contains(column.Name))
                {
                    index = 0;
                    column.Cells.Add(new WishEventHistoryPage_Cell { Index = index, Icon = column.Icon, Element = column.Element, Raity = column.Rarity });
                }
                else
                {
                    column.Cells.Add(new WishEventHistoryPage_Cell { Index = index, Element = column.Element, Raity = column.Rarity });
                }
            }
            if (column.Cells.Any(x => !string.IsNullOrWhiteSpace(x.Icon)))
            {
                column.CurrentIndex = index;
            }
            foreach (var cell in column.Cells)
            {
                if (!string.IsNullOrWhiteSpace(cell.Icon))
                {
                    break;
                }
                else
                {
                    cell.Index = 0;
                }
            }
            column.Cells.Reverse();
        }
        var maxIndex = columns.Where(x => x.Name != "刻晴").SelectMany(x => x.Cells).Max(x => x.Index);
        foreach (var column in columns)
        {
            foreach (var cell in column.Cells)
            {
                cell.Alpha = (double)cell.Index / maxIndex;
            }
        }
        Columns = columns.Where(x => x.CurrentIndex >= 0).OrderByDescending(x => x.Rarity).ThenByDescending(x => x.CurrentIndex).ToList();
        leftHeader.Reverse();
        LeftHeaders = leftHeader;
        TopHeaders = Columns.Select(x => new WishEventHistoryPage_TopHeader { Icon = x.Icon, Rarity = x.Rarity }).ToList();
    }


    private void LoadWeaponEventData()
    {
        using var litedb = DatabaseProvider.CreateLiteDB();
        var weapons = litedb.GetCollection<WeaponInfo>().FindAll().ToList();
        var wishevents = litedb.GetCollection<WishEventInfo>().FindAll().ToList();
        var weaevents = wishevents.Where(x => x.QueryType == WishType.WeaponEvent).GroupBy(x => x.StartTime).ToList();
        var leftHeader = weaevents.Select(x => new WishEventHistoryPage_LeftHeader
        {
            Version = x.First().Version,
            StartTime = x.First().StartTime,
            EndTime = x.First().EndTime,
            UpItems = x.SelectMany(y => y.Rank5UpItems).Concat(x.SelectMany(y => y.Rank4UpItems).Distinct()).ToList()
        }).ToList();
        var columns = weapons.Select(x => new WishEventHistoryPage_Column { Name = x.Name!, Icon = x.Icon!, Rarity = x.Rarity, Element = (ElementType)((int)x.WeaponType * 2) }).ToList();
        foreach (var column in columns)
        {
            int index = 0;
            foreach (var header in leftHeader)
            {
                index++;
                if (header.UpItems.Contains(column.Name))
                {
                    index = 0;
                    column.Cells.Add(new WishEventHistoryPage_Cell { Index = index, Icon = column.Icon, Element = column.Element, Raity = column.Rarity });
                }
                else
                {
                    column.Cells.Add(new WishEventHistoryPage_Cell { Index = index, Element = column.Element, Raity = column.Rarity });
                }
            }
            if (column.Cells.Any(x => !string.IsNullOrWhiteSpace(x.Icon)))
            {
                column.CurrentIndex = index;
            }
            foreach (var cell in column.Cells)
            {
                if (!string.IsNullOrWhiteSpace(cell.Icon))
                {
                    break;
                }
                else
                {
                    cell.Index = 0;
                }
            }
            column.Cells.Reverse();
        }
        var maxIndex = columns.Where(x => x.Name != "刻晴").SelectMany(x => x.Cells).Max(x => x.Index);
        foreach (var column in columns)
        {
            foreach (var cell in column.Cells)
            {
                cell.Alpha = (double)cell.Index / maxIndex;
            }
        }
        Columns = columns.Where(x => x.CurrentIndex >= 0).OrderByDescending(x => x.Rarity).ThenByDescending(x => x.CurrentIndex).ToList();
        leftHeader.Reverse();
        LeftHeaders = leftHeader;
        TopHeaders = Columns.Select(x => new WishEventHistoryPage_TopHeader { Icon = x.Icon, Rarity = x.Rarity }).ToList();
    }




    #region Scroll


    private bool canMoved;

    private Point oldPosition;


    private void _ScrollViewer_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        canMoved = true;
        oldPosition = e.GetCurrentPoint(_ScrollViewer).Position;
    }

    /// <summary>
    /// 鼠标拖动图片
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _ScrollViewer_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (canMoved)
        {
            var pointer = e.GetCurrentPoint(_ScrollViewer);
            if (pointer.Properties.IsLeftButtonPressed)
            {
                var deltaX = pointer.Position.X - oldPosition.X;
                var deltaY = pointer.Position.Y - oldPosition.Y;
                oldPosition = pointer.Position;
                // offset 的方向应与鼠标移动的方向相反
                // 不要使用 ChangeView，会出现图片无法跟随鼠标的情况
                _ScrollViewer.ScrollToHorizontalOffset(_ScrollViewer.HorizontalOffset - deltaX);
                _ScrollViewer.ScrollToVerticalOffset(_ScrollViewer.VerticalOffset - deltaY);
            }
        }
    }

    private void _ScrollViewer_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        canMoved = false;
    }


    #endregion




    [RelayCommand]
    private void Reset()
    {
        _ScrollViewer.ChangeView(0, 0, 1);
    }



    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            var scale = MainWindow.Current.UIScale;

            var backgroundBrush = this.ActualTheme == ElementTheme.Light ? new SolidColorBrush(Color.FromArgb(0xFF, 0xF3, 0xF3, 0xF3)) : new SolidColorBrush(Color.FromArgb(0xFF, 0x20, 0x20, 0x20));
            _ScrollViewer.Background = backgroundBrush;
            _Border_Content.Background = backgroundBrush;

            _ScrollViewer.ChangeView(0, 0, 1, true);
            await Task.Delay(60);

            var w = Math.Round(_ScrollViewer.ExtentWidth * scale);
            var h = Math.Round(_ScrollViewer.ExtentHeight * scale);


            CanvasDevice device = CanvasDevice.GetSharedDevice();
            using CanvasRenderTarget offscreen = new CanvasRenderTarget(device, width: (int)w, height: (int)h, dpi: 96);
            using (CanvasDrawingSession ds = offscreen.CreateDrawingSession())
            {
                // 因为 D3D11 的限制，渲染图像的宽高会缩放至不会超过 4096，绘制到目标图像时需要自行计算矩形区域大小
                // https://learn.microsoft.com/zh-cn/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.media.imaging.rendertargetbitmap?view=windows-app-sdk-1.2#remarks
                var r = new RenderTargetBitmap();

                await r.RenderAsync(_ScrollViewer);
                var buffer = await r.GetPixelsAsync();
                w = Math.Round(_ScrollViewer.ActualWidth * scale);
                h = Math.Round(_ScrollViewer.ActualHeight * scale);
                ds.DrawImage(CanvasBitmap.CreateFromBytes(device, buffer.ToArray(), r.PixelWidth, r.PixelHeight, Windows.Graphics.DirectX.DirectXPixelFormat.B8G8R8A8UIntNormalized), new Rect(0, 0, w, h));

                await r.RenderAsync(_Border_TopHeader);
                buffer = await r.GetPixelsAsync();
                w = Math.Round(_Border_TopHeader.ActualWidth * scale);
                h = Math.Round(_Border_TopHeader.ActualHeight * scale);
                ds.DrawImage(CanvasBitmap.CreateFromBytes(device, buffer.ToArray(), r.PixelWidth, r.PixelHeight, Windows.Graphics.DirectX.DirectXPixelFormat.B8G8R8A8UIntNormalized), new Rect(144 * scale, 0, w, h));

                await r.RenderAsync(_Border_LeftHeader);
                buffer = await r.GetPixelsAsync();
                w = Math.Round(_Border_LeftHeader.ActualWidth * scale);
                h = Math.Round(_Border_LeftHeader.ActualHeight * scale);
                ds.DrawImage(CanvasBitmap.CreateFromBytes(device, buffer.ToArray(), r.PixelWidth, r.PixelHeight, Windows.Graphics.DirectX.DirectXPixelFormat.B8G8R8A8UIntNormalized), new Rect(0, 60 * scale, w, h));

                await r.RenderAsync(_Border_Content);
                buffer = await r.GetPixelsAsync();
                w = Math.Round(_Border_Content.ActualWidth * scale);
                h = Math.Round(_Border_Content.ActualHeight * scale);
                ds.DrawImage(CanvasBitmap.CreateFromBytes(device, buffer.ToArray(), r.PixelWidth, r.PixelHeight, Windows.Graphics.DirectX.DirectXPixelFormat.B8G8R8A8UIntNormalized), new Rect(144 * scale, 60 * scale, w, h));
            }


            var picker = new FileSavePicker();
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.FileTypeChoices.Add("Jpg File", new List<string>() { ".jpg" });
            picker.SuggestedFileName = "WishEventHistory";
            InitializeWithWindow.Initialize(picker, MainWindow.Current.HWND);

            var file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                using var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
                await offscreen.SaveAsync(stream, CanvasBitmapFileFormat.Jpeg, 0.9f);
                stream.Dispose();
                NotificationProvider.ShowWithButton(InfoBarSeverity.Success, null, "已保存", "打开文件", async () => await Launcher.LaunchFileAsync(file), null, 3000);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
        finally
        {
            _ScrollViewer.Background = null;
            _Border_Content.Background = null;
        }
    }





}




public class WishEventHistoryPageCellToBackgroundBrushConverter : IValueConverter
{

    private static Color Pyro = Color.FromArgb(0xFF, 0xFD, 0x6E, 0x4D);
    private static Color Hydro = Color.FromArgb(0xFF, 0x2B, 0x67, 0xF2);
    private static Color Anemo = Color.FromArgb(0xFF, 0x2A, 0xB3, 0x9A);
    private static Color Electro = Color.FromArgb(0xFF, 0xA1, 0x7D, 0xFF);
    private static Color Dendro = Color.FromArgb(0xFF, 0xB1, 0xFD, 0x4D);
    private static Color Cryo = Color.FromArgb(0xFF, 0x8D, 0xDB, 0xFF);
    private static Color Geo = Color.FromArgb(0xFF, 0xCC, 0xBC, 0x71);
    private static SolidColorBrush Transparent = new SolidColorBrush { Color = Colors.Transparent, Opacity = 0 };


    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is WishEventHistoryPage_Cell cell)
        {
            var color = cell.Element switch
            {
                ElementType.Pyro => Pyro,
                ElementType.Hydro => Hydro,
                ElementType.Anemo => Anemo,
                ElementType.Electro => Electro,
                ElementType.Dendro => Dendro,
                ElementType.Cryo => Cryo,
                ElementType.Geo => Geo,
                _ => Colors.Transparent,
            };
            return new SolidColorBrush
            {
                Color = color,
                Opacity = cell.Alpha
            };
        }
        return Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}