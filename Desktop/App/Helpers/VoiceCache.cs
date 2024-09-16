using Scighost.WinUILib.Cache;
using Windows.Storage;

namespace Xunkong.Desktop.Helpers;

internal class VoiceCache : CacheBase<StorageFile>
{


    private static VoiceCache _instance;


    public static VoiceCache Instance => _instance ??= new VoiceCache { CacheDuration = TimeSpan.FromDays(90), RetryCount = 3 };


    protected override Task<StorageFile> InitializeTypeAsync(Stream stream)
    {
        throw new NotImplementedException();
    }


    protected override Task<StorageFile> InitializeTypeAsync(StorageFile baseFile)
    {
        return Task.FromResult(baseFile);
    }

}
