using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Threading;

namespace Xunkong.Desktop.Controls;

public sealed class CachedImage : ImageEx
{

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

                return new BitmapImage(imageUri);
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
                return new BitmapImage(new Uri(file.Path));
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
