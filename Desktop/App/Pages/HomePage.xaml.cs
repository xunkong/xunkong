using Microsoft.Graphics.Canvas.Effects;
using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Numerics;
using System.Runtime.InteropServices;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.System;
using Windows.UI.StartScreen;
using Xunkong.Desktop.Controls;
using Xunkong.Desktop.Summaries;
using Xunkong.Hoyolab;
using Xunkong.Hoyolab.Account;
using Xunkong.Hoyolab.Activity;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class HomePage : Page
{

    private const string FallbackWallpaperUri = "ms-appx:///Assets/Images/102203689_p0.jpg";
    public static readonly WallpaperInfoEx FallbackWallpaper = new WallpaperInfoEx
    {
        Id = 1702,
        Title = "原神2周年記念",
        Author = "アナ",
        Description = "おめでとうございます！\r\nこれからも旅人と共に、星と深淵を目指せ！",
        FileName = "[アナ] 原神2周年記念 [102203689_p0].jpg",
        Source = "https://www.pixiv.net/artworks/102203689",
        Url = "ms-appx:///Assets/Images/102203689_p0.jpg"
    };

    private readonly XunkongApiService _xunkongApiService;

    private readonly HoyolabService _hoyolabService;

    private readonly HoyolabClient _hoyolabClient;

    private readonly UpdateService _updateService;


    public HomePage()
    {
        this.InitializeComponent();
        _xunkongApiService = ServiceProvider.GetService<XunkongApiService>()!;
        _hoyolabService = ServiceProvider.GetService<HoyolabService>()!;
        _hoyolabClient = ServiceProvider.GetService<HoyolabClient>()!;
        _updateService = ServiceProvider.GetService<UpdateService>()!;
        _ = _hoyolabClient.UpdateDeviceFpAsync();
        Loaded += HomePage_Loaded;
    }



    [ObservableProperty]
    private WallpaperInfoEx currentWallpaper;


    [ObservableProperty]
    private List<GrowthScheduleItem> growthScheduleItems;


    [ObservableProperty]
    private List<Announcement> finishingActivities;


    [ObservableProperty]
    private ObservableCollection<GameAccount> gameAccounts;


    [ObservableProperty]
    private int gameServerIndex = AppSetting.GetValue<int>(SettingKeys.GameServerIndex);

    partial void OnGameServerIndexChanged(int value)
    {
        AppSetting.SetValue(SettingKeys.GameServerIndex, value);
    }


    private void HomePage_Loaded(object sender, RoutedEventArgs e)
    {
        // 推荐图片
        InitializeWallpaperAsync();
        // 游戏账号
        LoadGameAccounts();
        // 实时便笺
        GetDailyNotesAsync();
        // 今天刷什么
        GetCalendarAndGrowthScheduleAsync();
        // 即将结束的活动
        GetFinishingActivityAsync();
        // 通知
        GetNotificationContentAsync();
        // 更新
        CheckUpdateAsync();
        // 留影叙佳期
        //CheckBirthdayStarAsync();
        // 上传评分
        _xunkongApiService.UploadWallpaperRatingAsync();
    }



    #region Wallpaper


    /// <summary>
    /// 图片最大高度
    /// </summary>
    private double imageMaxHeight;
    /// <summary>
    /// 图片高宽比
    /// </summary>
    private double heightDivWidth;
    /// <summary>
    /// 图片 Visual
    /// </summary>
    private SpriteVisual imageVisual;


    /// <summary>
    /// 初始化推荐图片，并且下载新的
    /// </summary>
    private async void InitializeWallpaperAsync()
    {
        try
        {
            if (!AppSetting.GetValue(SettingKeys.EnableHomePageWallpaper, true))
            {
                return;
            }
            _Grid_Image.Visibility = Visibility.Visible;
            string? file = null;
            bool skipDownload = false;
            if (AppSetting.GetValue<bool>(SettingKeys.UseCustomWallpaper))
            {
                var path = Path.Combine(ApplicationData.Current.LocalCacheFolder.Path, "CustomWallpaper.png");
                if (File.Exists(path))
                {
                    file = path;
                    skipDownload = true;
                    CurrentWallpaper = new WallpaperInfoEx { Url = path, FileName = "CustomWallpaper.png" };
                }
            }
            if (string.IsNullOrWhiteSpace(file))
            {
                var wallpaper = _xunkongApiService.GetPreparedWallpaper();
                if (wallpaper is null)
                {
                    CurrentWallpaper = FallbackWallpaper;
                    file = (await StorageFile.GetFileFromApplicationUriAsync(new(FallbackWallpaperUri))).Path;
                }
                else
                {
                    var cachedFile = await XunkongCache.Instance.GetFileFromCacheAsync(new(wallpaper.Url));
                    if (cachedFile != null)
                    {
                        CurrentWallpaper = wallpaper;
                        file = cachedFile.Path;
                    }
                    else
                    {
                        CurrentWallpaper = FallbackWallpaper;
                        file = (await StorageFile.GetFileFromApplicationUriAsync(new(FallbackWallpaperUri))).Path;
                    }
                }
            }
            imageMaxHeight = MainWindow.Current.Height * 0.75 / MainWindow.Current.UIScale;
            _Grid_Image.MaxHeight = imageMaxHeight;
            await LoadBackgroundImage(file);
            if (NetworkHelper.IsInternetOnMeteredConnection() && !skipDownload)
            {
                if (!AppSetting.GetValue<bool>(SettingKeys.DownloadWallpaperOnMeteredInternet))
                {
                    // 使用按流量计费的网络时，不下载新图片
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "加载推荐图片");
        }
        finally
        {
            c_StackPanel_QuickAction.Visibility = Visibility.Visible;
        }
        Task.Run(async () =>
        {
            try
            {
                await _xunkongApiService.PrepareNextWallpaperAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "下载新图片");
            }
        }).ConfigureAwait(false).GetAwaiter();
    }

    /// <summary>
    /// 拖入图片
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _Grid_Image_DragOver(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems) || e.DataView.Contains(StandardDataFormats.Bitmap))
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.IsCaptionVisible = false;
            e.DragUIOverride.IsGlyphVisible = false;
        }
    }


    /// <summary>
    /// 拖入图片
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void _Grid_Image_Drop(object sender, DragEventArgs e)
    {
        try
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                var item = items.FirstOrDefault() as StorageFile;
                if (item != null)
                {
                    using var fs = await item.OpenReadAsync();
                    var decoder = await BitmapDecoder.CreateAsync(fs);
                    heightDivWidth = (double)decoder.PixelHeight / decoder.PixelWidth;
                    await LoadBackgroundImage(fs.AsStream());
                    CurrentWallpaper = null!;
                    var file = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync("CustomWallpaper.png", CreationCollisionOption.ReplaceExisting);
                    await item.CopyAndReplaceAsync(file);
                }
            }
            if (e.DataView.Contains(StandardDataFormats.Bitmap))
            {
                var r = await e.DataView.GetBitmapAsync();
                using var stream = await r.OpenReadAsync();
                var decoder = await BitmapDecoder.CreateAsync(stream);
                heightDivWidth = (double)decoder.PixelHeight / decoder.PixelWidth;
                await LoadBackgroundImage(stream.AsStream());
                CurrentWallpaper = null!;
                var file = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync("CustomWallpaper.png", CreationCollisionOption.ReplaceExisting);
                using var fs = await file.OpenStreamForWriteAsync();
                var s = stream.AsStream();
                s.Position = 0;
                await s.CopyToAsync(fs);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "拖入图片文件");
            NotificationProvider.Error("无法识别文件");
        }
    }



    /// <summary>
    /// 加载背景图片
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    private async Task LoadBackgroundImage(string file)
    {
        try
        {
            using var stream = File.OpenRead(file);
            var decoder = await BitmapDecoder.CreateAsync(stream.AsRandomAccessStream());
            heightDivWidth = (double)decoder.PixelHeight / decoder.PixelWidth;
            await LoadBackgroundImage(stream);
        }
        catch (COMException ex)
        {
            // 部分设备会因为缺少 Webp 解码器而出现 COMExeption 组件初始化失败。（0x88982F8B）
            // 还有可能会出现其他不知道的错误
            // 使用无透明度效果的图片代替，这个代替方法也没有用
            // 应该去应用商店更新解码组件
            Logger.Error(ex, "使用 Win2D 加载背景图片");
            NotificationProvider.Warning("图片加载失败", "缺少必要的解码组件，请在「设置-主页壁纸-图片格式」中查看。", 0);
        }
    }


    private async Task LoadBackgroundImage(Stream stream)
    {
        var ms = new MemoryStream();
        stream.Position = 0;
        await stream.CopyToAsync(ms);
        ms.Position = 0;

        var width = _Border_BackgroundImage.ActualWidth;
        var height = Math.Clamp(width * heightDivWidth, 0, imageMaxHeight);
        var size = new Size(width, height);

        var imageSurface = LoadedImageSurface.StartLoadFromStream(ms.AsRandomAccessStream(), size);
        var compositor = ElementCompositionPreview.GetElementVisual(_Border_BackgroundImage).Compositor;
        var imageBrush = compositor.CreateSurfaceBrush();
        imageBrush.Surface = imageSurface;
        imageBrush.Stretch = CompositionStretch.UniformToFill;
        imageBrush.VerticalAlignmentRatio = 0;

        var linearGradientBrush1 = compositor.CreateLinearGradientBrush();
        linearGradientBrush1.StartPoint = Vector2.Zero;
        linearGradientBrush1.EndPoint = Vector2.UnitY;
        linearGradientBrush1.ColorStops.Add(compositor.CreateColorGradientStop(0, Colors.White));
        linearGradientBrush1.ColorStops.Add(compositor.CreateColorGradientStop(0.95f, Colors.Black));

        var linearGradientBrush2 = compositor.CreateLinearGradientBrush();
        linearGradientBrush2.StartPoint = new Vector2(0.5f, 0);
        linearGradientBrush2.EndPoint = Vector2.UnitY;
        linearGradientBrush2.ColorStops.Add(compositor.CreateColorGradientStop(0, Colors.White));
        linearGradientBrush2.ColorStops.Add(compositor.CreateColorGradientStop(1, Colors.Black));

        var blendEffect = new BlendEffect
        {
            Mode = BlendEffectMode.Multiply,
            Background = new CompositionEffectSourceParameter("Background"),
            Foreground = new CompositionEffectSourceParameter("Foreground"),
        };

        var blendEffectFactory = compositor.CreateEffectFactory(blendEffect);
        var gradientEffectBrush = blendEffectFactory.CreateBrush();
        gradientEffectBrush.SetSourceParameter("Background", linearGradientBrush2);
        gradientEffectBrush.SetSourceParameter("Foreground", linearGradientBrush1);


        var invertEffect = new LuminanceToAlphaEffect
        {
            Source = new CompositionEffectSourceParameter("Source"),
        };

        var invertEffectFactory = compositor.CreateEffectFactory(invertEffect);
        var opacityEffectBrush = invertEffectFactory.CreateBrush();
        opacityEffectBrush.SetSourceParameter("Source", gradientEffectBrush);

        var maskEffect = new AlphaMaskEffect
        {
            AlphaMask = new CompositionEffectSourceParameter("Mask"),
            Source = new CompositionEffectSourceParameter("Source"),
        };

        var maskFactory = compositor.CreateEffectFactory(maskEffect);
        var maskEffectBrush = maskFactory.CreateBrush();
        maskEffectBrush.SetSourceParameter("Mask", opacityEffectBrush);
        maskEffectBrush.SetSourceParameter("Source", imageBrush);

        imageVisual = compositor.CreateSpriteVisual();
        imageVisual.Brush = maskEffectBrush;


        imageVisual.Size = new Vector2((float)width, (float)height);
        _Border_BackgroundImage.Height = height;
        _Border_BackgroundImage.Visibility = Visibility.Visible;
        ElementCompositionPreview.SetElementChildVisual(_Border_BackgroundImage, imageVisual);
    }


    /// <summary>
    /// 改变背景图片大小
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _Border_BackgroundImage_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        try
        {
            if (e.NewSize == e.PreviousSize)
            {
                return;
            }
            var width = _Border_BackgroundImage.ActualWidth;
            var height = Math.Clamp(width * heightDivWidth, 0, imageMaxHeight);
            if (imageVisual is not null)
            {
                imageVisual.Size = new Vector2((float)width, (float)height);
                _Border_BackgroundImage.Height = height;
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "改变背景图片大小");
        }
    }


    /// <summary>
    /// 浏览图片大图
    /// </summary>
    [RelayCommand]
    private void OpenImageViewer()
    {
        if (CurrentWallpaper is not null)
        {
            if (CurrentWallpaper == FallbackWallpaper)
            {
                MainWindow.Current.SetFullWindowContent(new ImageViewer { CurrentImage = FallbackWallpaper });
            }
            else
            {
                MainWindow.Current.SetFullWindowContent(new ImageViewer { CurrentImage = CurrentWallpaper, DecodeFromStream = true });
            }
            OperationHistory.AddToDatabase("OpenWallpaper", CurrentWallpaper.Id.ToString());
            Logger.TrackEvent("OpenWallpaper", "Id", CurrentWallpaper.Id.ToString());
        }
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
            if (CurrentWallpaper == FallbackWallpaper)
            {
                file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(FallbackWallpaperUri));
            }
            else
            {
                file = await XunkongCache.GetFileFromUriAsync(CurrentWallpaper?.Url);
            }
            if (file != null)
            {
                ClipboardHelper.SetBitmap(file);
                _Button_Copy.Content = "\xE8FB";
                await Task.Delay(3000);
                _Button_Copy.Content = "\xE8C8";
            }
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex, "复制图片");
            Logger.Error(ex, "复制图片");
        }
    }



    /// <summary>
    /// 保存图片
    /// </summary>
    [RelayCommand]
    private async Task SaveWallpaper()
    {
        if (string.IsNullOrWhiteSpace(CurrentWallpaper?.Url))
        {
            return;
        }
        try
        {
            StorageFile? file = null;
            if (CurrentWallpaper == FallbackWallpaper)
            {
                file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(FallbackWallpaperUri));
            }
            else
            {
                file = await XunkongCache.GetFileFromUriAsync(CurrentWallpaper?.Url);
            }
            if (file is null)
            {
                NotificationProvider.Warning("找不到缓存的文件", 3000);
                return;
            }
            var destFolder = AppSetting.GetValue<string>(SettingKeys.WallpaperSaveFolder) ?? Path.Combine(XunkongEnvironment.UserDataPath, "Wallpaper");
            var fileName = CurrentWallpaper?.FileName ?? Path.GetFileName(CurrentWallpaper?.Url)!;
            var destPath = Path.Combine(destFolder, fileName);
            Action openImageAction = () => Process.Start(new ProcessStartInfo { FileName = destPath, UseShellExecute = true });
            if (File.Exists(destPath))
            {
                NotificationProvider.ShowWithButton(InfoBarSeverity.Warning, "文件已存在", null, "打开文件", openImageAction, null, 3000);
            }
            else
            {
                Directory.CreateDirectory(destFolder);
                File.Copy(file.Path, destPath, true);
                NotificationProvider.ShowWithButton(InfoBarSeverity.Success, "已保存", fileName, "打开文件", openImageAction, null, 3000);
            }
            OperationHistory.AddToDatabase("SaveWallpaper", CurrentWallpaper?.Id.ToString());
            Logger.TrackEvent("SaveWallpaper", "Id", CurrentWallpaper?.Id.ToString());
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex, "保存图片");
            Logger.Error(ex, "保存图片");
        }
    }


    /// <summary>
    /// 打开图源
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task OpenImageSourceAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentWallpaper?.Source))
        {
            return;
        }
        try
        {
            await Launcher.LaunchUriAsync(new Uri(CurrentWallpaper.Source));
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex, "打开图源");
            Logger.Error(ex, "打开图源");
        }
    }


    /// <summary>
    /// 导航到图片历史记录页面
    /// </summary>
    [RelayCommand]
    private void NavigateToWallpaperHistoryPage()
    {
        MainPage.Current.Navigate(typeof(WallpaperHistoryPage));
    }


    private int lastRandomWallpaperId;

    /// <summary>
    /// 下一张图片
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task GetNextWallpaperAsync()
    {
        try
        {
            OperationHistory.AddToDatabase("GetNextWallpaper");
            Logger.TrackEvent("GetNextWallpaper");
            var loading = new ProgressRing { Width = 16, Height = 16 };
            _Button_NextWallpaper.Content = loading;
            var wallpaper = await _xunkongApiService.GetRandomWallpaperAsync();
            if (wallpaper != null)
            {
                if (wallpaper.Id == lastRandomWallpaperId)
                {
                    wallpaper = await _xunkongApiService.GetNextWallpaperAsync(CurrentWallpaper.Id);
                }
                else
                {
                    lastRandomWallpaperId = wallpaper.Id;
                }
                var uri = new Uri(wallpaper.Url);
                var fileTask = XunkongCache.Instance.GetFromCacheAsync(uri);
                var progress = XunkongCache.Instance.GetProgress(uri);
                if (progress != null)
                {
                    progress.ProgressChanged += (s, e) =>
                    {
                        if (e.DownloadState is Scighost.WinUILib.Cache.DownloadState.Completed)
                        {
                            loading.Value = 100;
                        }
                        else
                        {
                            loading.Value = e.BytesReceived * 100 / (double)e.TotalBytesToReceive;
                            if (loading.Value > 1)
                            {
                                loading.IsIndeterminate = false;
                            }
                        }
                    };
                }
                var file = await fileTask;
                loading.IsIndeterminate = true;
                if (file is not null)
                {
                    await LoadBackgroundImage(file.Path);
                    imageMaxHeight = MainWindow.Current.Height * 0.75 / MainWindow.Current.UIScale;
                    _Grid_Image.MaxHeight = imageMaxHeight;
                    CurrentWallpaper = wallpaper;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
        finally
        {
            _Button_NextWallpaper.Content = "\uE149";
        }
    }


    /// <summary>
    /// 保存评分
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void RatingControl_Wallpaper_ValueChanged(RatingControl sender, object args)
    {
        if (CurrentWallpaper != null)
        {
            XunkongApiService.SaveWallpaperRating(CurrentWallpaper);
        }
    }




    #endregion




    #region Content


    /// <summary>
    /// 实时便笺
    /// </summary>
    /// <returns></returns>
    private async void GetDailyNotesAsync()
    {
        try
        {
            if (AppSetting.GetValue(SettingKeys.EnableDailyNotesInHomePage, true))
            {
                var users = _hoyolabService.GetHoyolabUserInfoList();
                var roles = _hoyolabService.GetGenshinRoleInfoList(onlyEnableDailyNote: true);
                foreach (var role in roles)
                {
                    if (users.FirstOrDefault(x => x.Cookie == role.Cookie) is HoyolabUserInfo user)
                    {
                        DailyNoteThumbCard card;
                        try
                        {
                            var dailynote = await _hoyolabService.GetDailyNoteAsync(role);
                            card = new DailyNoteThumbCard { HoyolabUserInfo = user, GenshinRoleInfo = role, DailyNoteInfo = dailynote, };
                        }
                        catch (HoyolabException ex)
                        {
                            card = new DailyNoteThumbCard { HoyolabUserInfo = user, GenshinRoleInfo = role, Error = true, ErrorMessage = ex.Message, NeedVerification = ex.ReturnCode == 1034 };
                        }
                        c_TextBlock_DailyNote.Visibility = Visibility.Visible;
                        c_GridView_DailyNotes.Visibility = Visibility.Visible;
                        c_GridView_DailyNotes.Items.Add(card);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "实时便笺");
        }
    }


    /// <summary>
    /// 养成计划
    /// </summary>
    /// <returns></returns>
    private async void GetCalendarAndGrowthScheduleAsync()
    {
        try
        {
            using var docDb = DatabaseProvider.CreateDocDb();
            var col = docDb.GetCollection<GrowthScheduleItem>();
            var growthItems = col.FindAll().OrderBy(x => x.Order).ToList();
            if (!growthItems.Any())
            {
                return;
            }
            var todayTitles = new List<string>();
            try
            {
                var calendars = await _hoyolabClient.GetCalendarInfosAsync();
                var day = (int)DateTimeOffset.UtcNow.AddHours(4).DayOfWeek;
                day = day == 0 ? 7 : day;
                var dayString = day.ToString();
                var todayCalendars = calendars.Where(x => x.Kind == "2" && x.DropDay.Contains(dayString)).ToList();
                todayTitles = todayCalendars.Select(x => x.Title).ToList();
                foreach (var growItem in growthItems)
                {
                    if (todayCalendars.FirstOrDefault(x => x.Title == growItem.Name) is CalendarInfo info)
                    {
                        var materials = info.ContentInfos.Select(x => x.Title).ToList();
                        if (growItem.LevelCostItems?.Any() ?? false)
                        {
                            foreach (var costItem in growItem.LevelCostItems)
                            {
                                if (materials.Contains(costItem.Name))
                                {
                                    costItem.IsToday = true;
                                }
                            }
                        }
                        if (growItem.TalentCostItems?.Any() ?? false)
                        {
                            foreach (var costItem in growItem.TalentCostItems)
                            {
                                if (materials.Contains(costItem.Name))
                                {
                                    costItem.IsToday = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Logger.Error(ex, "养成计划");
            }
            GrowthScheduleItems = growthItems.OrderBy(x => x.HasTodayMaterial()).ThenBy(x => x.Order).ToList();
            c_TextBlock_GrowthSchedule.Visibility = Visibility.Visible;
            c_GridView_GrowthSchedule.Visibility = Visibility.Visible;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "养成计划");
        }
    }


    /// <summary>
    /// 单个材料完成
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void c_Button_CostItem_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is Button button)
            {
                if (button.Content is TextBlock textBlock)
                {
                    if (textBlock.DataContext is GrowthScheduleCostItem costItem)
                    {
                        costItem.IsFinish = !costItem.IsFinish;
                    }
                    if (VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(button))) is StackPanel stackPanel)
                    {
                        if (stackPanel.DataContext is GrowthScheduleItem item)
                        {
                            using var docDb = DatabaseProvider.CreateDocDb();
                            var col = docDb.GetCollection<GrowthScheduleItem>();
                            col.Upsert(item);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "单个材料完成");
        }
    }


    /// <summary>
    /// 即将结束的活动
    /// </summary>
    /// <returns></returns>
    private async void GetFinishingActivityAsync()
    {
        try
        {
            if (AppSetting.GetValue(SettingKeys.EnableFinishingActivityInHomePage, true))
            {
                var announces = await _hoyolabClient.GetGameAnnouncementsAsync();
                var activities = announces.Where(x => x.Type == 1 && x.IsFinishing).OrderBy(x => x.EndTime).ToList();
                if (activities.Any())
                {
                    c_TextBlock_FinishingActivity.Visibility = Visibility.Visible;
                    c_GridView_FinishingActivity.Visibility = Visibility.Visible;
                    FinishingActivities = activities;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "即将结束的活动");
        }
    }


    /// <summary>
    /// 显示活动内容
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void c_Grid_FinishingActivity_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        try
        {
            if (sender is Grid grid)
            {
                if (grid.DataContext is Announcement announce)
                {
                    c_AnnouncementContentViewer.Announce = announce;
                    FlyoutBase.ShowAttachedFlyout(c_Grid_Base);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "显示活动内容");
        }
    }


    #endregion




    #region 通知和检查更新


    private void AddToInfoBar(InfoBar infoBar, int? index = null)
    {
        _Grid_InfoBar.Visibility = Visibility.Visible;
        if (index is null)
        {
            _StackPanel_InfoBar.Children.Add(infoBar);
        }
        else
        {
            _StackPanel_InfoBar.Children.Insert(index.Value, infoBar);
        }
    }


    /// <summary>
    /// 初始化通知栏内容
    /// </summary>
    /// <returns></returns>
    private async void GetNotificationContentAsync()
    {
        try
        {
            var list = await _xunkongApiService.GetInfoBarContentListAsync();
            if (list?.Any() ?? false)
            {
                foreach (var item in list)
                {
                    InfoBar infoBar;
                    if (!string.IsNullOrWhiteSpace(item.ButtonContent) && !string.IsNullOrWhiteSpace(item.ButtonUri))
                    {
                        infoBar = NotificationProvider.Create((InfoBarSeverity)item.Severity, item.Title, item.Message, item.ButtonContent, async () => await Launcher.LaunchUriAsync(new Uri(item.ButtonUri)));
                    }
                    else
                    {
                        infoBar = NotificationProvider.Create((InfoBarSeverity)item.Severity, item.Title, item.Message);
                    }
                    AddToInfoBar(infoBar);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "主页通知栏内容");
        }
    }


    /// <summary>
    /// 检查更新
    /// </summary>
    /// <returns></returns>
    private async void CheckUpdateAsync()
    {
        try
        {
            var update = await _updateService.CheckUpdateAsync();
            if (update.Github is GithubService.GithubRelease release)
            {
                var infoBar = NotificationProvider.Create(InfoBarSeverity.Success, release.Prerelease ? $"新预览版 {release.TagName}" : $"新版本 {release.TagName}", release.Name, "详细信息", async () => await Launcher.LaunchUriAsync(new Uri(release.HtmlUrl)));
                AddToInfoBar(infoBar, 0);
            }
            if (update.Store is not null)
            {
                if (update.Store[0].Mandatory)
                {
                    var stack1 = new StackPanel { Spacing = 8 };
                    stack1.Children.Add(new TextBlock { Text = "这是一个强制更新版本" });
                    var stack2 = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
                    stack1.Children.Add(stack2);
                    var button1 = new Button { Content = "下载并安装" };
                    button1.Click += (_, _) =>
                    {
                        try
                        {
                            _updateService.RequestDownloadStoreNewVersion(update.Store);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, "下载并安装更新");
                        }
                    };
                    stack2.Children.Add(button1);
                    var button2 = new Button { Content = "打开商店" };
                    button2.Click += async (_, _) => await Launcher.LaunchUriAsync(new(XunkongEnvironment.StoreProtocolUrl));
                    stack2.Children.Add(button2);
                    var dialog = new ContentDialog
                    {
                        Title = "有新版本",
                        Content = stack1,
                        XamlRoot = MainWindow.Current.XamlRoot,
                        RequestedTheme = MainWindow.Current.ActualTheme,
                    };
                    await dialog.ShowWithZeroMarginAsync();
                }
                else
                {
                    var infoBar = NotificationProvider.Create(InfoBarSeverity.Success, "薛定谔的更新", "只有安装后才知道是不是真的更新", "下载并安装", () => _updateService.RequestDownloadStoreNewVersion(update.Store));
                    AddToInfoBar(infoBar, 0);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "主页检查更新");
        }
    }




    /// <summary>
    /// 留影叙佳期
    /// </summary>
    private async void CheckBirthdayStarAsync()
    {
        try
        {
            if (AppSetting.GetValue(SettingKeys.EnableBirthdayStarInHomePage, false))
            {
                var roles = _hoyolabService.GetGenshinRoleInfoList();
                foreach (var role in roles)
                {
                    try
                    {
                        var index = await _hoyolabService.CheckBirthdayStarAsync(role);
                        if (index != null)
                        {
                            var name = string.Join("和", index.Role.Select(x => x.Name));
                            var text = $"今天是{name}的生日，快点去庆祝吧！";
                            const string url = "https://webstatic.mihoyo.com/ys/event/e20220303-birthday/index.html?game_biz=hk4e_cn&bbs_presentation_style=fullscreen&bbs_auth_required=true&bbs_landscape=true&activity_id=20220301153521";
                            var infoBar = NotificationProvider.Create(InfoBarSeverity.Success, $"留影叙佳期 - {role.Nickname}", text, "为TA庆祝",
                                () => MainPage.Current.Navigate(typeof(WebViewPage), new WebViewPage.NavigateParameter("BirthdayStar", url, role)));
                            AddToInfoBar(infoBar);
                        }
                    }
                    catch (HoyolabException ex)
                    {
                        Logger.Error(ex);
                        var infoBar = NotificationProvider.Create(InfoBarSeverity.Error, $"留影叙佳期 - {role.Nickname}", ex.Message);
                        AddToInfoBar(infoBar);
                    }

                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            var infoBar = NotificationProvider.Create(InfoBarSeverity.Error, "留影叙佳期", ex.Message);
            AddToInfoBar(infoBar);
        }
    }



    #endregion




    #region 快捷操作


    [RelayCommand]
    private async Task StartGameAsync()
    {
        if (await GameAccountService.StartGameAsync(GameServerIndex, true))
        {
            await InvokeService.CheckTransformerReachedAndHomeCoinFullAsync(true);
        }
    }


    [RelayCommand]
    private void OpenSummary2022()
    {
        MainWindow.Current.SetFullWindowContent(new Summary2022View());
    }




    private void LoadGameAccounts()
    {
        try
        {
            GameAccounts = new(GameAccountService.GetGameAccountsFromDatabase());
            ChangeJumpList();
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }



    private async void ChangeJumpList()
    {
        try
        {
            var logo = new Uri("ms-appx:///Assets/Logos/StoreLogo.png");
            var jumpList = await JumpList.LoadCurrentAsync();
            jumpList.Items.Clear();
            var firstItem = JumpListItem.CreateWithArguments("StartGame", "启动游戏");
            firstItem.Logo = logo;
            jumpList.Items.Add(firstItem);
            foreach (var account in GameAccounts)
            {
                var item = JumpListItem.CreateWithArguments($"StartGameWithAccount_{account.SHA256}", $"{account.Name}（{account.Server.ToDescription()}）");
                item.Logo = logo;
                item.Description = "切换账号并启动游戏";
                jumpList.Items.Add(item);
            }
            await jumpList.SaveAsync();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "JumpList");
        }
    }



    private async void Button_ChangeAccountAndStartGame_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is Button button && button.DataContext is GameAccount account)
            {
                var serverName = account.Server.ToDescription();
                if (GameAccountService.IsGameRunning((int)account.Server))
                {
                    NotificationProvider.Warning($"{serverName}游戏正在运行，无法切换账号");
                    return;
                }
                if (!GameAccountService.ChangeGameAccount(account))
                {
                    NotificationProvider.Warning($"{serverName}账号切换失败");
                    return;
                }
                await GameAccountService.StartGameAsync((int)account.Server);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
    }



    private void Button_ChangeAccount_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is Button button && button.DataContext is GameAccount account)
            {
                var serverName = account.Server.ToDescription();
                if (GameAccountService.IsGameRunning((int)account.Server))
                {
                    NotificationProvider.Warning($"{serverName}游戏正在运行，无法切换账号");
                    return;
                }
                if (GameAccountService.ChangeGameAccount(account))
                {
                    NotificationProvider.Success($"{serverName}账号已切换为 {account.Name}");
                }
                else
                {
                    NotificationProvider.Warning($"{serverName}账号切换失败");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
    }




    private void MenuFlyoutItem_DeleteAccount_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is MenuFlyoutItem item && item.DataContext is GameAccount account)
            {
                GameAccountService.DeleteGameAccount(account);
                if (GameAccounts.Contains(account))
                {
                    GameAccounts.Remove(account);
                    ChangeJumpList();
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
    }




    [RelayCommand]
    private async Task AddGameAccountAsync()
    {
        try
        {
            var accounts = GameAccountService.GetGameAccountsFromRegistry();
            if (accounts.Any())
            {
                var dialog = new ContentDialog
                {
                    Title = "保存游戏账号",
                    Content = new AddGameAccountDialog { GameAccounts = accounts },
                    CloseButtonText = "关闭",
                    XamlRoot = MainWindow.Current.XamlRoot,
                    RequestedTheme = MainWindow.Current.ActualTheme,
                };
                await dialog.ShowWithZeroMarginAsync();
                LoadGameAccounts();
            }
            else
            {
                NotificationProvider.Warning("没有找到已登录的游戏账号");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
    }



    [RelayCommand]
    private async Task BackupGameScreenshotAsync()
    {
        try
        {
            OperationHistory.AddToDatabase("BackupScreenshot");
            Logger.TrackEvent("BackupScreenshot");
            int addCount = 0;
            var backFolder = AppSetting.GetValue<string>(SettingKeys.GameScreenshotBackupFolder);
            if (string.IsNullOrWhiteSpace(backFolder))
            {
                backFolder = Path.Combine(XunkongEnvironment.UserDataPath, "Screenshot");
            }
            var result = await Task.Run<(int AddCount, int TotalCount)>(() =>
            {
                if (!Directory.Exists(backFolder))
                {
                    Directory.CreateDirectory(backFolder);
                }
                for (int i = 0; i < 2; i++)
                {
                    try
                    {
                        var gameExe = GameAccountService.GetGameExePath(i);
                        var screenshotFolder = Path.Join(Path.GetDirectoryName(gameExe), "ScreenShot");
                        if (Directory.Exists(screenshotFolder))
                        {
                            var files = Directory.GetFiles(screenshotFolder, "*.png");
                            foreach (var file in files)
                            {
                                var path = Path.Combine(backFolder, Path.GetFileName(file));
                                if (!File.Exists(path))
                                {
                                    File.Copy(file, path, true);
                                    File.SetCreationTime(path, new FileInfo(file).CreationTime);
                                    addCount++;
                                }
                            }
                        }
                    }
                    catch (XunkongException)
                    {
                        continue;
                    }
                }
                var cloudFolder = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "GenshinImpactCloudGame");
                if (Directory.Exists(cloudFolder))
                {
                    var files = Directory.GetFiles(cloudFolder, "*.png");
                    foreach (var file in files)
                    {
                        var path = Path.Combine(backFolder, Path.GetFileName(file));
                        if (!File.Exists(path))
                        {
                            File.Copy(file, path, true);
                            File.SetCreationTime(path, new FileInfo(file).CreationTime);
                            addCount++;
                        }
                    }
                }
                var totalFiles = Directory.GetFiles(backFolder).Length;
                return (addCount, totalFiles);
            });
            NotificationProvider.ShowWithButton(InfoBarSeverity.Success, "备份完成", $"新增图片 {result.AddCount} 张，总计图片 {result.TotalCount} 张。", "打开文件夹", async () => await Launcher.LaunchFolderPathAsync(backFolder), null, 5000);
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
    }






    #endregion


}
