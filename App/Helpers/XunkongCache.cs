using Scighost.WinUILib.Cache;
using System.Net.Http;
using Windows.Storage;

namespace Xunkong.Desktop.Helpers;

internal sealed class XunkongCache : CacheBase<StorageFile>
{


    private static XunkongCache _instance;


    public static XunkongCache Instance => _instance ??= new XunkongCache { CacheDuration = TimeSpan.FromDays(30), RetryCount = 3 };


    private static string UA = $"XunkongDesktop/{XunkongEnvironment.AppVersion}";


    protected override Task<StorageFile> InitializeTypeAsync(Stream stream)
    {
        throw new NotImplementedException();
    }

    protected override Task<StorageFile> InitializeTypeAsync(StorageFile baseFile)
    {
        return Task.FromResult(baseFile);
    }

    protected override HttpRequestMessage GetHttpRequestMessage(Uri uri)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        if (uri.Host is "file.xunkong.cc")
        {
            request.Headers.Add("UserAgent", UA);
        }
        return request;
    }


    public static async Task<StorageFile?> GetFileFromUriAsync(Uri uri)
    {
        try
        {
            return uri.Scheme switch
            {
                "ms-appx" => await StorageFile.GetFileFromApplicationUriAsync(uri),
                "file" => await StorageFile.GetFileFromPathAsync(uri.ToString()),
                _ => await Instance.GetFileFromCacheAsync(uri),
            };
        }
        catch
        {
            return null;
        }
    }


    public static async Task<StorageFile?> GetFileFromUriAsync(string? url)
    {
        if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
        {
            return await GetFileFromUriAsync(uri);
        }
        else
        {
            return null;
        }
    }




}