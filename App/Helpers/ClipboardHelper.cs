using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Xunkong.Desktop.Helpers;

internal static class ClipboardHelper
{

    public static void SetText(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            var data = new DataPackage();
            data.RequestedOperation = DataPackageOperation.Copy;
            data.SetText(value);
            Clipboard.SetContent(data);
            Clipboard.Flush();
        }
    }


    public static void SetBitmap(IRandomAccessStream stream)
    {
        var value = RandomAccessStreamReference.CreateFromStream(stream);
        var data = new DataPackage();
        data.RequestedOperation = DataPackageOperation.Copy;
        data.SetBitmap(value);
        Clipboard.SetContent(data);
        Clipboard.Flush();
    }


    public static void SetBitmap(IStorageFile file)
    {
        var value = RandomAccessStreamReference.CreateFromFile(file);
        var data = new DataPackage();
        data.RequestedOperation = DataPackageOperation.Copy;
        data.SetBitmap(value);
        Clipboard.SetContent(data);
        Clipboard.Flush();
    }


    public static void SetBitmap(Uri uri)
    {
        var value = RandomAccessStreamReference.CreateFromUri(uri);
        var data = new DataPackage();
        data.RequestedOperation = DataPackageOperation.Copy;
        data.SetBitmap(value);
        Clipboard.SetContent(data);
        Clipboard.Flush();
    }


}