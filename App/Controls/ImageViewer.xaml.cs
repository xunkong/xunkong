using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Controls;


/// <summary>
/// 图片查看器
/// </summary>
/// <remarks>
/// <see cref="Image"/> 在解码后属性 <see cref="FrameworkElement.ActualWidth"/>和<see cref="FrameworkElement.ActualHeight"/> 实际上是图片像素的宽高；
/// 再经过系统的缩放后，在显示器上实际占用的像素值会比图片像素要多，一定程度上造成了图片模糊；
/// 查看器中通过引入系统缩放率 <see cref="MainWindow.UIScale"/> 解决这个问题。
/// </remarks>
[INotifyPropertyChanged]
public sealed partial class ImageViewer : UserControl
{

    private readonly double uiScale;


    public ImageViewer()
    {
        this.InitializeComponent();
        uiScale = MainWindow.Current.UIScale;
        _ScrollViewer_Image.MaxZoomFactor = (float)(2 / uiScale);
        switch (AppSetting.GetValue<int>(SettingKeys.ImageViewerRequestTheme))
        {
            case 1:
                RequestedTheme = ElementTheme.Light;
                RadioMenuFlyoutItem_Theme_Light.IsChecked = true;
                break;
            case 2:
                RequestedTheme = ElementTheme.Dark;
                RadioMenuFlyoutItem_Theme_Dark.IsChecked = true;
                break;
            default:
                RequestedTheme = ElementTheme.Default;
                RadioMenuFlyoutItem_Theme_Default.IsChecked = true;
                break;
        }
    }



    [ObservableProperty]
    private WallpaperInfoEx currentImage;


    [ObservableProperty]
    private List<WallpaperInfoEx>? imageCollection;


    public bool DecodeFromStream { get; set; }


    public bool ShowLoadingRing { get; set; }


    partial void OnCurrentImageChanged(WallpaperInfoEx value)
    {
        _ScrollViewer_Image.HorizontalScrollMode = ScrollMode.Disabled;
        _ScrollViewer_Image.VerticalScrollMode = ScrollMode.Disabled;
        if (value.Id > 0)
        {
            Grid_WallpaperInfo.Visibility = Visibility.Visible;
        }
        else
        {
            Grid_WallpaperInfo.Visibility = Visibility.Collapsed;
        }
        if (ShowLoadingRing)
        {
            _ScrollViewer_Image.ZoomToFactor((float)(1 / uiScale));
            _ScrollViewer_Image.ScrollToHorizontalOffset(_ScrollViewer_Image.ExtentWidth / 2 - _ScrollViewer_Image.ViewportWidth / 2);
            _ScrollViewer_Image.ScrollToVerticalOffset(_ScrollViewer_Image.ExtentHeight / 2 - _ScrollViewer_Image.ViewportHeight / 2);
        }
    }


    partial void OnImageCollectionChanged(List<WallpaperInfoEx>? value)
    {
        if (value?.Any() ?? false)
        {
            _GridView_ImageCollection.Visibility = Visibility.Visible;
        }
        else
        {
            _GridView_ImageCollection.Visibility = Visibility.Collapsed;
        }
    }



    /// <summary>
    /// 缩放
    /// </summary>
    /// <param name="factor">缩放因子</param>
    private void Zoom(double factor)
    {
        var newFactor = Math.Clamp(factor, 0.1, 2 / uiScale);
        var oldFactor = _ScrollViewer_Image.ZoomFactor;
        var offset_Width = _ScrollViewer_Image.HorizontalOffset;
        var offset_Height = _ScrollViewer_Image.VerticalOffset;
        var viewport_Width = _ScrollViewer_Image.ViewportWidth;
        var viewport_Height = _ScrollViewer_Image.ViewportHeight;
        var extent_Width = _ScrollViewer_Image.ExtentWidth;
        var extent_Height = _ScrollViewer_Image.ExtentHeight;
        //var extent_Width_New = extent_Width * newFactor / oldFactor;
        //var extent_Height_New = extent_Height * newFactor / oldFactor;

        // 保证图片以画面中心为基点进行缩放
        double offset_Width_New = 0, offfset_Height_New = 0;
        if (extent_Width < viewport_Width)
        {
            // 如果缩放前图片宽度 < 视图宽度，则以图片半宽为基准缩放
            offset_Width_New = extent_Width / 2 * newFactor / oldFactor - viewport_Width / 2;
        }
        else
        {
            // 如果缩放前图片宽度 > 视图宽度，则以视图中心点到图片边缘为基准缩放
            offset_Width_New = (viewport_Width / 2 + offset_Width) * newFactor / oldFactor - viewport_Width / 2;
        }
        if (extent_Height < viewport_Height)
        {
            offfset_Height_New = extent_Height / 2 * newFactor / oldFactor - viewport_Height / 2;
        }
        else
        {
            offfset_Height_New = (viewport_Height / 2 + offset_Height) * newFactor / oldFactor - viewport_Height / 2;
        }

        // 缩放后若填不满视图，则图片居中（这部分没用）
        //if (extent_Width_New < viewport_Width)
        //{
        //    offset_Width_New = (extent_Width_New - viewport_Width) / 2;
        //}
        //if (extent_Height_New < viewport_Height)
        //{
        //    offfset_Height_New = (extent_Height_New - viewport_Height) / 2;
        //}

        _ScrollViewer_Image.ChangeView(offset_Width_New, offfset_Height_New, (float)newFactor);
    }



    /// <summary>
    /// 缩小
    /// </summary>
    [RelayCommand]
    private void ZoomOut()
    {
        // 缩放率调整为 0.1 的倍数
        var factor = Math.Ceiling(_ScrollViewer_Image.ZoomFactor * 10 * uiScale - 1.1f) / 10 / uiScale;
        Zoom(factor);
    }


    /// <summary>
    /// 放大
    /// </summary>
    [RelayCommand]
    private void ZoomIn()
    {
        var factor = Math.Floor(_ScrollViewer_Image.ZoomFactor * 10 * uiScale + 1.1f) / 10 / uiScale;
        Zoom(factor);
    }




    /// <summary>
    /// 顺时针旋转90°
    /// </summary>
    [RelayCommand]
    private void Rotate()
    {
        _Image.Rotation = _Image.Rotation + 90;
    }


    /// <summary>
    /// 复制图片
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task CopyImageAsync()
    {
        try
        {
            StorageFile? file = null;
            var uri = new Uri(CurrentImage.Url);
            if (uri.Scheme is "ms-appx")
            {
                file = await StorageFile.GetFileFromApplicationUriAsync(uri);
            }
            else if (uri.Scheme is "file")
            {
                file = await StorageFile.GetFileFromPathAsync(uri.ToString());
            }
            else
            {
                file = await XunkongCache.Instance.GetFileFromCacheAsync(uri);
            }
            if (file is null)
            {
                NotificationProvider.Warning("找不到缓存的文件", 3000);
            }
            else
            {
                ClipboardHelper.SetBitmap(file);
                _Button_Copy.Content = "\xE8FB";
                await Task.Delay(3000);
                _Button_Copy.Content = "\xE8C8";
            }
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex);
            Logger.Error(ex, "复制图片");
        }
    }


    /// <summary>
    /// 保存图片
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task SaveImageAsync()
    {
        try
        {
            var file = await XunkongCache.GetFileFromUriAsync(CurrentImage.Url);
            if (file is null)
            {
                NotificationProvider.Warning("找不到缓存的文件", 3000);
            }
            else
            {
                var picker = new FileSavePicker();
                picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                var extension = Path.GetExtension(CurrentImage.Url);
                if (string.IsNullOrWhiteSpace(extension)) { extension = ".png"; }
                picker.FileTypeChoices.Add("Image", new List<string>() { extension });
                picker.SuggestedFileName = Path.GetFileName(CurrentImage.Url);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, MainWindow.Current.HWND);
                var saveFile = await picker.PickSaveFileAsync();
                if (saveFile != null)
                {
                    await file.CopyAndReplaceAsync(saveFile);
                    NotificationProvider.ShowWithButton(InfoBarSeverity.Success, "已保存", saveFile.Name, "打开文件", async () => await Launcher.LaunchFileAsync(saveFile), null, 3000);
                }
            }
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex);
            Logger.Error(ex, "保存图片");
        }
    }



    /// <summary>
    /// 关闭页面
    /// </summary>
    [RelayCommand]
    private void Close()
    {
        MainWindow.Current.CloseFullWindowContent();
    }



    /// <summary>
    /// 缩放率文本变更
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _ScrollViewer_Image_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
    {
        _TextBlock_Factor.Text = (_ScrollViewer_Image.ZoomFactor * uiScale).ToString("P0");
    }



    /// <summary>
    /// 图片加载后计算合适的缩放率
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _Image_ImageOpened(object sender, RoutedEventArgs _)
    {
        _ScrollViewer_Image.HorizontalScrollMode = ScrollMode.Enabled;
        _ScrollViewer_Image.VerticalScrollMode = ScrollMode.Enabled;
        var width = _Image.ActualWidth;
        var height = _Image.ActualHeight;
        if (width * height == 0)
        {
            return;
        }
        _Image.CenterPoint = new System.Numerics.Vector3((float)(width / 2), (float)(height / 2), 0);
        var factor = GetFitZoomFactor();
        _TextBlock_Factor.Text = (factor * uiScale).ToString("P0");
        _ScrollViewer_Image.ZoomToFactor((float)factor);
    }

    /// <summary>
    /// 图片加载后计算合适的缩放率（部分无法触发 <see cref="MenuImage.ImageOpened"/> 的情况）
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _Image_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        _ScrollViewer_Image.HorizontalScrollMode = ScrollMode.Enabled;
        _ScrollViewer_Image.VerticalScrollMode = ScrollMode.Enabled;
        var width = _Image.ActualWidth;
        var height = _Image.ActualHeight;
        if (width * height == 0)
        {
            return;
        }
        _Image.CenterPoint = new System.Numerics.Vector3((float)(width / 2), (float)(height / 2), 0);
        var factor = GetFitZoomFactor();
        _TextBlock_Factor.Text = (factor * uiScale).ToString("P0");
        _ScrollViewer_Image.ZoomToFactor((float)factor);
    }

    /// <summary>
    /// 计算适合窗口的图片缩放率
    /// </summary>
    /// <returns></returns>
    private double GetFitZoomFactor()
    {
        double factor;
        if (_Image.Rotation % 180 == 0)
        {
            var widthFactor = _ScrollViewer_Image.ViewportWidth / (_Image.ActualWidth + 1);
            var heightFactor = _ScrollViewer_Image.ViewportHeight / (_Image.ActualHeight + 1);
            factor = Math.Min(widthFactor, heightFactor);
        }
        else
        {
            var widthFactor = _ScrollViewer_Image.ViewportHeight / (_Image.ActualWidth + 1);
            var heightFactor = _ScrollViewer_Image.ViewportWidth / (_Image.ActualHeight + 1);
            factor = Math.Min(widthFactor, heightFactor);
        }
        return Math.Min(factor, 1 / uiScale);
    }


    /// <summary>
    /// 鼠标单击，显示或隐藏图片缩率图栏
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _ScrollViewer_Image_Tapped(object sender, TappedRoutedEventArgs e)
    {
        ChangeToolBarVisibility();
    }


    /// <summary>
    /// 更改工具栏的可见性
    /// </summary>
    private void ChangeToolBarVisibility()
    {
        if (_Border_ToolBar.IsHitTestVisible)
        {
            _Border_ToolBar.Opacity = 0;
            _Border_ToolBar.IsHitTestVisible = false;
            if (ImageCollection?.Any() ?? false)
            {
                _GridView_ImageCollection.Opacity = 0;
                _GridView_ImageCollection.IsHitTestVisible = false;
            }
            if (CurrentImage.Id > 0)
            {
                Grid_WallpaperInfo.Opacity = 0;
                Grid_WallpaperInfo.IsHitTestVisible = false;
            }
        }
        else
        {
            _Border_ToolBar.Opacity = 1;
            _Border_ToolBar.IsHitTestVisible = true;
            if (ImageCollection?.Any() ?? false)
            {
                _GridView_ImageCollection.Opacity = 1;
                _GridView_ImageCollection.IsHitTestVisible = true;
            }
            if (CurrentImage.Id > 0)
            {
                Grid_WallpaperInfo.Opacity = 1;
                Grid_WallpaperInfo.IsHitTestVisible = true;
            }
        }
    }


    /// <summary>
    /// 鼠标双击，放大或缩小
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _ScrollViewer_Image_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        ChangeToolBarVisibility();
        var oldFactor = _ScrollViewer_Image.ZoomFactor;
        double newFactor;
        if (oldFactor < 0.4 / uiScale)
        {
            newFactor = oldFactor * 2;
        }
        else if (oldFactor < 0.999 / uiScale)
        {
            newFactor = 1 / uiScale;
        }
        else
        {
            newFactor = GetFitZoomFactor();
        }
        Zoom(newFactor);
    }


    /// <summary>
    /// 鼠标滚动，切换图片
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _ScrollViewer_Image_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        var stats = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control);
        if (stats is CoreVirtualKeyStates.None or CoreVirtualKeyStates.Locked)
        {
            if (ImageCollection?.Any() ?? false)
            {
                _GridView_ImageCollection.Focus(FocusState.Programmatic);
                var index = _GridView_ImageCollection.SelectedIndex;
                var count = ImageCollection.Count;
                var pointer = e.GetCurrentPoint(_ScrollViewer_Image);
                if (pointer.Properties.MouseWheelDelta < 0)
                {
                    if (index < count - 1)
                    {
                        _GridView_ImageCollection.SelectedIndex = index + 1;
                    }
                }
                else
                {
                    if (index > 0)
                    {
                        _GridView_ImageCollection.SelectedIndex = index - 1;
                    }
                }
            }
            // 阻止事件传递到滚动条
            e.Handled = true;
        }
    }



    private bool canImageMoved;

    private Point oldPosition;


    private void _ScrollViewer_Image_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        canImageMoved = true;
        oldPosition = e.GetCurrentPoint(_ScrollViewer_Image).Position;
    }

    /// <summary>
    /// 鼠标拖动图片
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _ScrollViewer_Image_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (canImageMoved)
        {
            var pointer = e.GetCurrentPoint(_ScrollViewer_Image);
            if (pointer.Properties.IsLeftButtonPressed)
            {
                var deltaX = pointer.Position.X - oldPosition.X;
                var deltaY = pointer.Position.Y - oldPosition.Y;
                oldPosition = pointer.Position;
                // offset 的方向应与鼠标移动的方向相反
                // 不要使用 ChangeView，会出现图片无法跟随鼠标的情况
                _ScrollViewer_Image.ScrollToHorizontalOffset(_ScrollViewer_Image.HorizontalOffset - deltaX);
                _ScrollViewer_Image.ScrollToVerticalOffset(_ScrollViewer_Image.VerticalOffset - deltaY);
            }
        }
    }

    private void _ScrollViewer_Image_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        canImageMoved = false;
    }



    /// <summary>
    /// 保存评分
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void RatingControl_Wallpaper_ValueChanged(RatingControl sender, object args)
    {
        if (CurrentImage != null)
        {
            XunkongApiService.SaveWallpaperRating(CurrentImage);
        }
    }


    /// <summary>
    /// 展开壁纸信息卡片
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Grid_WallpaperInfo_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        Grid_WallpaperInfo.ClearValue(WidthProperty);
        Grid_WallpaperInfo.ClearValue(HeightProperty);
        var maxWidth = (this.ActualWidth - _Border_ToolBar.ActualWidth) / 2 - 96;
        if (maxWidth > 200)
        {
            Grid_WallpaperInfo.MaxWidth = maxWidth;
        }
    }


    /// <summary>
    /// 缩小壁纸信息卡片
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Grid_WallpaperInfo_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        Grid_WallpaperInfo.Width = 200;
        Grid_WallpaperInfo.Height = 120;
        Grid_WallpaperInfo.ClearValue(MaxWidthProperty);
    }



    /// <summary>
    /// 打开图源网址
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task OpenImageSourceUrlAsync()
    {
        try
        {
            if (CurrentImage?.Source != null)
            {
                await Launcher.LaunchUriAsync(new Uri(CurrentImage.Source));
            }
        }
        catch { }
    }



    /// <summary>
    /// 修改图片浏览器主题
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RadioMenuFlyoutItem_Theme_Click(object sender, RoutedEventArgs e)
    {
        if (sender is RadioMenuFlyoutItem item)
        {
            switch (item.Tag)
            {
                case "Light":
                    RequestedTheme = ElementTheme.Light;
                    RadioMenuFlyoutItem_Theme_Light.IsChecked = true;
                    AppSetting.SetValue(SettingKeys.ImageViewerRequestTheme, 1);
                    break;
                case "Dark":
                    RequestedTheme = ElementTheme.Dark;
                    RadioMenuFlyoutItem_Theme_Dark.IsChecked = true;
                    AppSetting.SetValue(SettingKeys.ImageViewerRequestTheme, 2);
                    break;
                default:
                    RequestedTheme = ElementTheme.Default;
                    RadioMenuFlyoutItem_Theme_Default.IsChecked = true;
                    AppSetting.SetValue(SettingKeys.ImageViewerRequestTheme, 0);
                    break;
            }
        }
    }




}
