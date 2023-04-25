using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections.Concurrent;
using System.Threading;

namespace Xunkong.Desktop.Controls;

public sealed class CachedImage : ImageEx
{



    public bool EnableThumbnail
    {
        get { return (bool)GetValue(EnableThumbnailProperty); }
        set { SetValue(EnableThumbnailProperty, value); }
    }

    // Using a DependencyProperty as the backing store for EnableThumbnail.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty EnableThumbnailProperty =
        DependencyProperty.Register("EnableThumbnail", typeof(bool), typeof(CachedImage), new PropertyMetadata(false));





    private static readonly ConcurrentDictionary<Uri, Uri> fileCache = new();


    public static void ClearCache() => fileCache.Clear();



    protected override async Task<ImageSource> ProvideCachedResourceAsync(Uri imageUri, CancellationToken token)
    {
        try
        {
            if (imageUri.Scheme is "ms-appx")
            {
                return new BitmapImage(imageUri);
            }
            else if (imageUri.Scheme is "file")
            {
                if (EnableThumbnail)
                {
                    var thumb = await ThumbnailCache.GetThumbnailAsync(imageUri.LocalPath);
                    if (!string.IsNullOrWhiteSpace(thumb))
                    {
                        return new BitmapImage(new(thumb));
                    }
                }
                return new BitmapImage(imageUri);
            }
            else
            {
                if (fileCache.TryGetValue(imageUri, out var uri))
                {
                    return new BitmapImage(uri);
                }
                else
                {

                    var file = await XunkongCache.Instance.GetFromCacheAsync(imageUri, false, token);
                    if (token.IsCancellationRequested)
                    {
                        throw new TaskCanceledException("Image source has changed.");
                    }
                    if (file is null)
                    {
                        throw new FileNotFoundException(imageUri.ToString());
                    }
                    uri = new Uri(file.Path);
                    fileCache[imageUri] = uri;
                    return new BitmapImage(uri);
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




}
