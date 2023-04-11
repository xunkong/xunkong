// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Scighost.WinUILib.Cache;
using System.Threading;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Controls;

[INotifyPropertyChanged]
public sealed partial class MenuImage : UserControl
{



    private const string LoadingState = "Loading";

    private const string LoadedState = "Loaded";

    private const string UnloadedState = "Unloaded";

    private const string FailedState = "Failed";


    private static Dictionary<string, long> _fileSizeCache = new();




    public MenuImage()
    {
        this.InitializeComponent();
    }



    #region Property




    public ImageSource PlaceholderSource
    {
        get { return (ImageSource)GetValue(PlaceholderSourceProperty); }
        set { SetValue(PlaceholderSourceProperty, value); }
    }

    // Using a DependencyProperty as the backing store for PlaceHolderSource.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty PlaceholderSourceProperty =
        DependencyProperty.Register("PlaceholderSource", typeof(ImageSource), typeof(MenuImage), new PropertyMetadata(null));



    public Stretch PlaceholderStretch
    {
        get { return (Stretch)GetValue(PlaceholderStretchProperty); }
        set { SetValue(PlaceholderStretchProperty, value); }
    }

    // Using a DependencyProperty as the backing store for PlaceholderStretch.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty PlaceholderStretchProperty =
        DependencyProperty.Register("PlaceholderStretch", typeof(Stretch), typeof(MenuImage), new PropertyMetadata(Stretch.None));




    public string? PlaceholderText
    {
        get { return (string)GetValue(PlaceholderTextProperty); }
        set { SetValue(PlaceholderTextProperty, value); }
    }

    // Using a DependencyProperty as the backing store for PlaceholderText.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty PlaceholderTextProperty =
        DependencyProperty.Register("PlaceholderText", typeof(string), typeof(MenuImage), new PropertyMetadata(null));





    public ImageSource FailedSource
    {
        get { return (ImageSource)GetValue(FailedSourceProperty); }
        set { SetValue(FailedSourceProperty, value); }
    }

    // Using a DependencyProperty as the backing store for FallbackSource.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty FailedSourceProperty =
        DependencyProperty.Register("FailedSource", typeof(ImageSource), typeof(MenuImage), new PropertyMetadata(null));




    public Stretch FailedStretch
    {
        get { return (Stretch)GetValue(FailedStretchProperty); }
        set { SetValue(FailedStretchProperty, value); }
    }

    // Using a DependencyProperty as the backing store for FallbackStretch.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty FailedStretchProperty =
        DependencyProperty.Register("FailedStretch", typeof(Stretch), typeof(MenuImage), new PropertyMetadata(Stretch.Uniform));



    public Stretch Stretch
    {
        get { return (Stretch)GetValue(StretchProperty); }
        set { SetValue(StretchProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Stretch.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty StretchProperty =
        DependencyProperty.Register("Stretch", typeof(Stretch), typeof(MenuImage), new PropertyMetadata(Stretch.Uniform));





    public object? Source
    {
        get { return GetValue(SourceProperty); }
        set { SetValue(SourceProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register("Source", typeof(object), typeof(MenuImage), new PropertyMetadata(null, SourceChanged));



    public bool DisableAnimation
    {
        get { return (bool)GetValue(DisableAnimationProperty); }
        set { SetValue(DisableAnimationProperty, value); }
    }

    // Using a DependencyProperty as the backing store for DisableAnimation.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty DisableAnimationProperty =
        DependencyProperty.Register("DisableAnimation", typeof(bool), typeof(MenuImage), new PropertyMetadata(false));




    public bool ShowLoadingRing
    {
        get { return (bool)GetValue(ShowLoadingRingProperty); }
        set { SetValue(ShowLoadingRingProperty, value); }
    }

    // Using a DependencyProperty as the backing store for ShowLoadingRing.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ShowLoadingRingProperty =
        DependencyProperty.Register("ShowLoadingRing", typeof(bool), typeof(MenuImage), new PropertyMetadata(false));




    public bool ClearHeightOnLoaded
    {
        get { return (bool)GetValue(ClearHeightOnLoadedProperty); }
        set { SetValue(ClearHeightOnLoadedProperty, value); }
    }

    // Using a DependencyProperty as the backing store for ClearWidthOnLoaded.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ClearHeightOnLoadedProperty =
        DependencyProperty.Register("ClearHeightOnLoaded", typeof(bool), typeof(MenuImage), new PropertyMetadata(false));




    public bool DecodeFromStream
    {
        get { return (bool)GetValue(DecodeFromStreamProperty); }
        set { SetValue(DecodeFromStreamProperty, value); }
    }

    // Using a DependencyProperty as the backing store for DecodeFromStream.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty DecodeFromStreamProperty =
        DependencyProperty.Register("DecodeFromStream", typeof(bool), typeof(MenuImage), new PropertyMetadata(false));





    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PixelInfoString))]
    private int pixelHeight;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PixelInfoString))]
    private int pixelWidth;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PixelInfoString))]
    private long fileSize;


    public string PixelInfoString => $"{PixelWidth} x {PixelHeight}   {fileSize / 1024:N0} KB";


    public event RoutedEventHandler ImageOpened;

    public event ExceptionRoutedEventHandler ImageFailed;


    #endregion




    #region Source



    private static void SourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = d as MenuImage;

        if (control == null)
        {
            return;
        }

        if (e.OldValue == null || e.NewValue == null || !e.OldValue.Equals(e.NewValue))
        {
            control.SetSource(e.NewValue);
        }
    }


    private CancellationTokenSource _tokenSource;


    private async void SetSource(object? source)
    {

        _tokenSource?.Cancel();

        _tokenSource = new CancellationTokenSource();


        if (source == null)
        {
            AttachSource(null);
            return;
        }

        GoToState(LoadingState);

        var imageSource = source as ImageSource;
        if (imageSource != null)
        {
            AttachSource(imageSource);

            return;
        }

        var uri = source as Uri;
        if (uri == null)
        {
            var url = source as string ?? source.ToString();
            if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri))
            {
                GoToState(FailedState);
                return;
            }
        }

        if (!IsHttpUri(uri) && !uri.IsAbsoluteUri)
        {
            uri = new Uri("ms-appx:///" + uri.OriginalString.TrimStart('/'));
        }

        try
        {
            await LoadImageAsync(uri, _tokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            // nothing to do as cancellation has been requested.
        }
        catch (Exception e)
        {
            GoToState(FailedState);
        }
    }


    private static bool IsHttpUri(Uri uri)
    {
        return uri.IsAbsoluteUri && (uri.Scheme == "http" || uri.Scheme == "https");
    }


    private void AttachSource(ImageSource? source)
    {
        // Setting the source at this point should call ImageExOpened/VisualStateManager.GoToState
        // as we register to both the ImageOpened/ImageFailed events of the underlying control.
        // We only need to call those methods if we fail in other cases before we get here.

        Image.Source = source;


        if (source == null)
        {
            GoToState(UnloadedState);
        }
        else if (source is BitmapSource { PixelHeight: > 0, PixelWidth: > 0 })
        {
            GoToState(LoadedState);
        }
    }



    private async Task LoadImageAsync(Uri imageUri, CancellationToken token)
    {
        if (imageUri != null)
        {
            if (string.Equals(imageUri.Scheme, "data", StringComparison.OrdinalIgnoreCase))
            {
                var source = imageUri.OriginalString;
                const string base64Head = "base64,";
                var index = source.IndexOf(base64Head);
                if (index >= 0)
                {
                    var bytes = Convert.FromBase64String(source.Substring(index + base64Head.Length));
                    var bitmap = new BitmapImage();
                    await bitmap.SetSourceAsync(new MemoryStream(bytes).AsRandomAccessStream());

                    if (!_tokenSource.IsCancellationRequested)
                    {
                        AttachSource(bitmap);
                    }
                }
            }
            else
            {
                var img = await ProvideCachedResourceAsync(imageUri, token);

                if (!_tokenSource.IsCancellationRequested)
                {
                    // Only attach our image if we still have a valid request.
                    AttachSource(img);
                }
            }
        }
    }



    private async Task<ImageSource> ProvideCachedResourceAsync(Uri imageUri, CancellationToken token)
    {
        try
        {
            if (imageUri.Scheme is "ms-appx")
            {
                if (!_fileSizeCache.TryGetValue(imageUri.ToString(), out long size))
                {
                    var file = await StorageFile.GetFileFromApplicationUriAsync(imageUri);
                    size = new FileInfo(file.Path).Length;
                    _fileSizeCache[imageUri.ToString()] = size;
                }
                FileSize = size;
                return new BitmapImage(imageUri);
            }
            else if (imageUri.Scheme is "file")
            {
                if (DecodeFromStream)
                {
                    using var stream = File.OpenRead(imageUri.LocalPath);
                    var bitmap = new BitmapImage();
                    await bitmap.SetSourceAsync(stream.AsRandomAccessStream());
                    PixelHeight = bitmap.PixelHeight;
                    PixelWidth = bitmap.PixelWidth;
                    FileSize = stream.Length;
                    return bitmap;
                }
                else
                {
                    if (!_fileSizeCache.TryGetValue(imageUri.ToString(), out long size))
                    {
                        size = new FileInfo(imageUri.LocalPath).Length;
                        _fileSizeCache[imageUri.ToString()] = size;
                    }
                    FileSize = size;
                    return new BitmapImage(imageUri);
                }
            }
            else
            {
                var fileTask = XunkongCache.Instance.GetFromCacheAsync(imageUri, false, token);
                if (ShowLoadingRing)
                {
                    var progress = XunkongCache.Instance.GetProgress(imageUri);
                    if (progress != null)
                    {
                        _progress = progress;
                        progress.ProgressChanged += Progress_ProgressChanged;
                    }
                }
                var file = await fileTask;
                if (token.IsCancellationRequested)
                {
                    throw new TaskCanceledException("Image source has changed.");
                }
                if (file is null)
                {
                    throw new FileNotFoundException(imageUri.ToString());
                }
                if (!_fileSizeCache.TryGetValue(imageUri.ToString(), out long size))
                {
                    size = new FileInfo(file.Path).Length;
                    _fileSizeCache[imageUri.ToString()] = size;
                }
                FileSize = size;
                if (DecodeFromStream)
                {
                    using var stream = await file.OpenReadAsync();
                    var bitmap = new BitmapImage();
                    bitmap.ImageOpened += (s, e) => ImageOpened?.Invoke(this, e);
                    await bitmap.SetSourceAsync(stream);
                    PixelHeight = bitmap.PixelHeight;
                    PixelWidth = bitmap.PixelWidth;
                    return bitmap;
                }
                else
                {
                    return new BitmapImage(new Uri(file.Path));
                }
            }
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (FileNotFoundException)
        {
            throw;
        }
        catch (Exception)
        {
            await XunkongCache.Instance.RemoveAsync(new[] { imageUri });
            throw;
        }
    }


    private Progress<DownloadProgress>? _progress;

    private void Progress_ProgressChanged(object? sender, DownloadProgress e)
    {
        if (sender is Progress<DownloadProgress> p && sender != _progress)
        {
            p.ProgressChanged -= Progress_ProgressChanged;
            return;
        }
        if (e.DownloadState is DownloadState.Pending)
        {
            LoadingProgressRing.IsIndeterminate = true;
        }
        if (e.DownloadState is DownloadState.Downloading)
        {
            LoadingProgressRing.IsIndeterminate = false;
            var percentage = (double)e.BytesReceived / e.TotalBytesToReceive * 100;
            if (percentage > 0)
            {
                LoadingProgressRing.Value = percentage;
            }
        }
        if (e.DownloadState is DownloadState.Completed)
        {
            LoadingProgressRing.IsIndeterminate = true;
        }
    }


    private void Image_ImageOpened(object sender, RoutedEventArgs e)
    {
        GoToState(LoadedState);
        if (ClearHeightOnLoaded)
        {
            ClearValue(HeightProperty);
        }
        if (sender is Image image && image.Source is BitmapSource source)
        {
            PixelHeight = source.PixelHeight;
            PixelWidth = source.PixelWidth;
        }
        ImageOpened?.Invoke(this, e);
    }


    private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
    {
        GoToState(FailedState);
        ImageFailed?.Invoke(this, e);
    }



    private void GoToState(string state)
    {
        if (state == LoadedState && DisableAnimation)
        {
            VisualStateManager.GoToState(this, "LoadedDisableAnimation", true);
        }
        else
        {
            VisualStateManager.GoToState(this, state, true);
        }
    }



    #endregion




    #region Command


    /// <summary>
    /// 刷新
    /// </summary>
    [RelayCommand]
    private void Refresh()
    {
        SetSource(Source);
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
            if (Source is string url)
            {
                var file = await XunkongCache.GetFileFromUriAsync(url);
                if (file != null)
                {
                    ClipboardHelper.SetBitmap(file);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }


    /// <summary>
    /// 复制图片链接
    /// </summary>
    [RelayCommand]
    private void CopyLink()
    {
        try
        {
            if (Source is string { Length: > 0 } url)
            {
                ClipboardHelper.SetText(url);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }


    /// <summary>
    /// 复制文件
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task CopyFileAsync()
    {
        try
        {
            if (Source is string url)
            {
                var file = await XunkongCache.GetFileFromUriAsync(url);
                if (file != null)
                {
                    var rf = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(Path.GetFileName(url), CreationCollisionOption.ReplaceExisting);
                    await file.CopyAndReplaceAsync(rf);
                    ClipboardHelper.SetStorageItems(DataPackageOperation.Copy, rf);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }


    /// <summary>
    /// 另存为
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task SaveAsAsync()
    {
        try
        {
            if (Source is string url)
            {
                if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
                {
                    var extension = Path.GetExtension(uri.ToString());
                    if (string.IsNullOrWhiteSpace(extension)) { extension = ".png"; }
                    var file = await XunkongCache.GetFileFromUriAsync(uri);
                    if (file != null)
                    {
                        var picker = new FileSavePicker();
                        picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                        picker.FileTypeChoices.Add("Image", new List<string> { extension });
                        picker.SuggestedFileName = Path.GetFileName(uri.ToString());
                        WinRT.Interop.InitializeWithWindow.Initialize(picker, MainWindow.Current.HWND);
                        var saveFile = await picker.PickSaveFileAsync();
                        if (saveFile != null)
                        {
                            await file.CopyAndReplaceAsync(saveFile);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }








    #endregion





}
