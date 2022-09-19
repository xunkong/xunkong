using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Scighost.WinUILib.Cache;
using System.Threading;

namespace Xunkong.Desktop.Controls;

internal class CachedImage : ImageEx
{


    static CachedImage()
    {
        ImageCache.Instance.CacheDuration = TimeSpan.FromDays(30);
        ImageCache.Instance.RetryCount = 3;
    }


    protected override async Task<ImageSource> ProvideCachedResourceAsync(Uri imageUri, CancellationToken token)
    {
        try
        {
            if (imageUri.Scheme is "file" or "ms-appx")
            {
                return new BitmapImage(imageUri);
            }
            var image = await ImageCache.Instance.GetFromCacheAsync(imageUri, false, token);
            if (token.IsCancellationRequested)
            {
                throw new TaskCanceledException("Image source has changed.");
            }
            if (image is null)
            {
                throw new FileNotFoundException(imageUri.ToString());
            }
            return image;
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            await ImageCache.Instance.RemoveAsync(new[] { imageUri });
            throw;
        }

    }


}
