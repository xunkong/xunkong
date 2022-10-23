using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Threading;

namespace Xunkong.Desktop.Controls;

internal class CachedImage : ImageEx
{


    protected override async Task<ImageSource> ProvideCachedResourceAsync(Uri imageUri, CancellationToken token)
    {
        try
        {
            if (imageUri.Scheme is "file" or "ms-appx")
            {
                return new BitmapImage(imageUri);
            }
            var file = await XunkongCache.Instance.GetFromCacheAsync(imageUri, false, token);
            if (file is null)
            {
                throw new FileNotFoundException(imageUri.ToString());
            }
            if (token.IsCancellationRequested)
            {
                throw new TaskCanceledException("Image source has changed.");
            }
            return new BitmapImage(new Uri(file.Path));
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (FileNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await XunkongCache.Instance.RemoveAsync(new[] { imageUri });
            throw;
        }

    }


}
