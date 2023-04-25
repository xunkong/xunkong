using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Xunkong.Desktop.Helpers;

internal abstract class ThumbnailCache
{


    private static List<KeyValuePair<string, BitmapTypedValue>> EncodingOptions = new List<KeyValuePair<string, BitmapTypedValue>>
    { new KeyValuePair<string, BitmapTypedValue>("ImageQuality", new BitmapTypedValue(0.8, Windows.Foundation.PropertyType.Single)) };

    private static StorageFolder ThumbFolder;

    private static SemaphoreSlim Semaphore = new SemaphoreSlim(Math.Clamp(Environment.ProcessorCount / 2, 1, 4));

    private static Dictionary<string, string> _cache = new();


    public static async Task<StorageFolder> GetCacheFolderAsync()
    {
        if (ThumbFolder is null)
        {
            ThumbFolder = await ApplicationData.Current.TemporaryFolder.CreateFolderAsync("ThumbCache", CreationCollisionOption.OpenIfExists);
        }
        return ThumbFolder;
    }


    public static void ClearCache()
    {
        _cache.Clear();
    }


    public static async Task<string?> GetThumbnailAsync(string path, CancellationToken? cancellationToken = null)
    {
        if (ThumbFolder is null)
        {
            ThumbFolder = await ApplicationData.Current.TemporaryFolder.CreateFolderAsync("ThumbCache", CreationCollisionOption.OpenIfExists);
        }
        if (_cache.TryGetValue(path, out var thumb))
        {
            return thumb;
        }
        var name = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(path)));
        var item = await ThumbFolder.TryGetItemAsync(name);
        if (item != null)
        {
            _cache[path] = item.Path;
            return item.Path;
        }
        try
        {
            await Semaphore.WaitAsync(cancellationToken ?? CancellationToken.None);
            var t = await GenerateThumbnailAsync(path, name, cancellationToken ?? CancellationToken.None);
            if (!string.IsNullOrWhiteSpace(t))
            {
                _cache[path] = t;
            }
            return t;
        }
        catch (Exception ex)
        {
            return null;
        }
        finally
        {
            Semaphore.Release();
        }
    }



    private static async Task<string?> GenerateThumbnailAsync(string path, string name, CancellationToken? cancellationToken = null)
    {
        var item = await ThumbFolder.TryGetItemAsync(name);
        if (item != null)
        {
            return item.Path;
        }
        var file = await StorageFile.GetFileFromPathAsync(path);
        using var fs = await file.OpenReadAsync();
        var decoder = await BitmapDecoder.CreateAsync(fs);
        var pixel = await decoder.GetPixelDataAsync();
        var ms = new InMemoryRandomAccessStream();
        var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, ms, EncodingOptions);
        encoder.SetPixelData(decoder.BitmapPixelFormat, decoder.BitmapAlphaMode, decoder.PixelWidth, decoder.PixelHeight, decoder.DpiX, decoder.DpiY, pixel.DetachPixelData());
        encoder.BitmapTransform.ScaledWidth = 400;
        encoder.BitmapTransform.ScaledHeight = 400 * decoder.PixelHeight / decoder.PixelWidth;
        encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Fant;
        await encoder.FlushAsync();
        if (ms.Size == 0)
        {
            return null;
        }
        var thumb = await ThumbFolder.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);
        using var ts = await thumb.OpenAsync(FileAccessMode.ReadWrite);
        ms.Seek(0);
        await RandomAccessStream.CopyAndCloseAsync(ms, ts);
        return thumb.Path;
    }



}
