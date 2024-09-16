using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace Xunkong.Hoyolab.Wishlog;

/// <summary>
/// 祈愿记录请求类
/// </summary>
public class WishlogClient
{

    private const string CnUrl = "https://public-operation-hk4e.mihoyo.com/gacha_info/api/getGachaLog";

    private const string SeaUrl = "https://hk4e-api-os.hoyoverse.com/gacha_info/api/getGachaLog";

    private static readonly string UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    private static readonly string LogFile_Cn = Path.Combine(UserProfile, @"AppData\LocalLow\miHoYo\原神\output_log.txt");

    private static readonly string LogFile_Sea = Path.Combine(UserProfile, @"AppData\LocalLow\miHoYo\Genshin Impact\output_log.txt");

    private readonly HttpClient _httpClient;



    public WishlogClient(HttpClient? httpClient = null)
    {
        if (httpClient is null)
        {
            _httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.All });
        }
        else
        {
            _httpClient = httpClient;
        }
    }



    /// <summary>
    /// 从原神的日志文件获取祈愿记录网址（仅 Windows 可用）
    /// </summary>
    /// <param name="isSea">是否外服优先</param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException">没有找到日志文件，或没有从日志文件中找到祈愿记录网址</exception>
    [Obsolete("日志文件中不再有链接", true)]
    public static async Task<string> GetWishlogUrlFromLogFileAsync(bool isSea = false)
    {
        var file = isSea ? LogFile_Sea : LogFile_Cn;
        if (!File.Exists(file))
        {
            file = isSea ? LogFile_Cn : LogFile_Sea;
        }
        if (!File.Exists(file))
        {
            throw new FileNotFoundException("Cannot find log file of Genshin Impact.");
        }
        using var stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream);
        var log = await reader.ReadToEndAsync();
        var matches = Regex.Matches(log, @"OnGetWebViewPageFinish:(.+#/log)");
        if (matches.Any())
        {
            return matches.Last().Value.Replace("OnGetWebViewPageFinish:", "");
        }
        else
        {
            throw new FileNotFoundException("Cannot find wishlog url from log file of Genshin Impact.");
        }
    }



    /// <summary>
    /// 从原神的浏览器缓存文件获取祈愿记录网址
    /// </summary>
    /// <param name="exePath">原神游戏本体 exe 路径</param>
    /// <returns></returns>
    public static string? GetWishlogUrlFromCacheFile(string exePath)
    {
        string? file = null;
        var match = ReadOnlySpan<byte>.Empty;
        if (exePath.EndsWith("YuanShen.exe"))
        {
            var matcher = new Matcher();
            matcher.AddInclude(@"YuanShen_Data\webCaches\Cache\Cache_Data\data_2");
            matcher.AddInclude(@"YuanShen_Data\webCaches\*\Cache\Cache_Data\data_2");
            var result = matcher.Execute(new DirectoryInfoWrapper(new FileInfo(exePath).Directory!));
            var files = result.Files.Select(x => Path.Combine(Path.GetDirectoryName(exePath)!, x.Path));
            file = files.OrderByDescending(x => new FileInfo(x).LastWriteTime).FirstOrDefault();
            match = "https://webstatic.mihoyo.com/hk4e/event/e20190909gacha-v3/index.html"u8;
        }
        if (exePath.EndsWith("GenshinImpact.exe"))
        {
            var matcher = new Matcher();
            matcher.AddInclude(@"GenshinImpact_Data\webCaches\Cache\Cache_Data\data_2");
            matcher.AddInclude(@"GenshinImpact_Data\webCaches\*\Cache\Cache_Data\data_2");
            var result = matcher.Execute(new DirectoryInfoWrapper(new FileInfo(exePath).Directory!));
            var files = result.Files.Select(x => Path.Combine(Path.GetDirectoryName(exePath)!, x.Path));
            file = files.OrderByDescending(x => new FileInfo(x).LastWriteTime).FirstOrDefault();
            match = "https://webstatic-sea.hoyoverse.com/genshin/event/e20190909gacha-v3/index.html"u8;
        }
        if (File.Exists(file))
        {
            using var fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            var ms = new MemoryStream();
            fs.CopyTo(ms);
            var span = ms.ToArray().AsSpan();
            var index = span.LastIndexOf(match);
            if (index >= 0)
            {
                var length = span[index..].IndexOf("\0"u8);
                return Encoding.UTF8.GetString(span.Slice(index, length));
            }
        }
        return null;
    }





    /// <summary>
    /// 合并网址和身份验证相关的查询参数
    /// </summary>
    /// <param name="wishlogUrl"></param>
    /// <returns></returns>
    /// <exception cref="HoyolabException"></exception>
    private static string GetBaseAndAuthString(string wishlogUrl)
    {
        var match = Regex.Match(wishlogUrl, @"(https://webstatic[!-z]+)");
        if (match.Success)
        {
            wishlogUrl = match.Groups[1].Value;
            var auth = wishlogUrl.Substring(wishlogUrl.IndexOf('?')).Replace("#/log", "");
            return CnUrl + auth;
        }
        match = Regex.Match(wishlogUrl, @"(https://gs.hoyoverse.com[!-z]+)");
        if (match.Success)
        {
            wishlogUrl = match.Groups[1].Value;
            var auth = wishlogUrl.Substring(wishlogUrl.IndexOf('?')).Replace("#/log", "");
            return SeaUrl + auth;
        }
        match = Regex.Match(wishlogUrl, @"(https://public-operation-hk4e[!-z]+)");
        if (!match.Success)
        {
            match = Regex.Match(wishlogUrl, @"(https://hk4e-api[!-z]+)");
        }
        if (match.Success)
        {
            wishlogUrl = match.Groups[1].Value;
            wishlogUrl = Regex.Replace(wishlogUrl, @"&gacha_type=\d", "");
            wishlogUrl = Regex.Replace(wishlogUrl, @"&page=\d", "");
            wishlogUrl = Regex.Replace(wishlogUrl, @"&size=\d", "");
            wishlogUrl = Regex.Replace(wishlogUrl, @"&end_id=\d", "");
            return wishlogUrl;
        }
        throw new HoyolabException(-1, "Url does not fit the requirement.");
    }


    /// <summary>
    /// 获取一页祈愿记录
    /// </summary>
    /// <param name="baseString"></param>
    /// <param name="param"></param>
    /// <returns>没有数据时返回空集合</returns>
    /// <exception cref="HoyolabException"></exception>
    private async Task<List<WishlogItem>> GetWishlogByParamAsync(string baseString, QueryParam param)
    {
        var url = $"{baseString}&{param}";
        await Task.Delay(Random.Shared.Next(200, 300));
#if NativeAOT
        var response = await _httpClient.GetFromJsonAsync(url, WishlogJsonContext.Default.HoyolabBaseWrapperWishlogWrapper);
#else
        var response = await _httpClient.GetFromJsonAsync<HoyolabBaseWrapper<WishlogWrapper>>(url);
#endif
        if (response is null)
        {
            throw new HoyolabException(-1, "Cannot parse the return data.");
        }
        if (response.ReturnCode != 0)
        {
            throw new HoyolabException(response.ReturnCode, response.Message ?? "No return meesage.");
        }
        return response.Data?.List ?? new(0);
    }


    /// <summary>
    /// 获取一种卡池类型的祈愿数据
    /// </summary>
    /// <param name="queryType"></param>
    /// <param name="lastId">获取的祈愿id小于最新id即停止</param>
    /// <param name="size">每次api请求获取几条数据，不超过20，默认6</param>
    /// <returns>没有数据返回空集合</returns>
    /// <exception cref="HoyolabException">api请求返回值不为零时抛出异常</exception>
    private async Task<List<WishlogItem>> GetWishlogByTypeAsync(string baseString, WishType queryType, long lastId = 0, int size = 20, IProgress<(WishType WishType, int Page)>? progress = null)
    {
        var param = new QueryParam(queryType, 1, size);
        var result = new List<WishlogItem>();
        while (true)
        {
            progress?.Report((queryType, param.Page));
            var list = await GetWishlogByParamAsync(baseString, param);
            result.AddRange(list);
            if (list.Count == size && list.Last().Id > lastId)
            {
                param.Page++;
                param.EndId = list.Last().Id;
            }
            else
            {
                break;
            }
        }
        foreach (var item in result)
        {
            item.QueryType = queryType;
        }
        return result;
    }


    /// <summary>
    /// 获取所有的祈愿数据
    /// </summary>
    /// <param name="wishlogUrl"></param>
    /// <param name="lastId"></param>
    /// <param name="size">一次获取几条记录，[1,20]</param>
    /// <returns>以 id 顺序排列，没有数据返回空集合</returns>
    public async Task<List<WishlogItem>> GetAllWishlogAsync(string wishlogUrl, long lastId = 0, int size = 20, IProgress<(WishType QueryType, int Page)>? progress = null)
    {
        var baseUrl = GetBaseAndAuthString(wishlogUrl);
        lastId = lastId < 0 ? 0 : lastId;
        size = Math.Clamp(size, 1, 20);
        var result = new List<WishlogItem>();
        result.AddRange(await GetWishlogByTypeAsync(baseUrl, WishType.Novice, lastId, size, progress));
        result.AddRange(await GetWishlogByTypeAsync(baseUrl, WishType.Permanent, lastId, size, progress));
        result.AddRange(await GetWishlogByTypeAsync(baseUrl, WishType.CharacterEvent, lastId, size, progress));
        result.AddRange(await GetWishlogByTypeAsync(baseUrl, WishType.WeaponEvent, lastId, size, progress));
        result.AddRange(await GetWishlogByTypeAsync(baseUrl, WishType.ChronicledWish, lastId, size, progress));
        return result.OrderBy(x => x.Id).ToList();
    }


    /// <summary>
    /// 获取祈愿记录网址关联的 uid
    /// </summary>
    /// <param name="wishlogUrl"></param>
    /// <returns>没有祈愿记录则返回 0</returns>
    public async Task<int> GetUidAsync(string wishlogUrl)
    {
        var baseUrl = GetBaseAndAuthString(wishlogUrl);
        var param = new QueryParam(WishType.CharacterEvent, 1);
        var list = await GetWishlogByParamAsync(baseUrl, param);
        if (list.Any())
        {
            return list.First().Uid;
        }
        param.WishType = WishType.Permanent;
        list = await GetWishlogByParamAsync(baseUrl, param);
        if (list.Any())
        {
            return list.First().Uid;
        }
        param.WishType = WishType.WeaponEvent;
        list = await GetWishlogByParamAsync(baseUrl, param);
        if (list.Any())
        {
            return list.First().Uid;
        }
        param.WishType = WishType.ChronicledWish;
        list = await GetWishlogByParamAsync(baseUrl, param);
        if (list.Any())
        {
            return list.First().Uid;
        }
        param.WishType = WishType.Novice;
        list = await GetWishlogByParamAsync(baseUrl, param);
        if (list.Any())
        {
            return list.First().Uid;
        }
        return 0;
    }



    public async Task<WishlogItemWiki> GetWishlogItemWikiAsync(CancellationToken cancellationToken = default)
    {
        const string url = "https://api-takumi.mihoyo.com/event/platsimulator/config?gids=2&game=hk4e";
        var wrapper = await _httpClient.GetFromJsonAsync<HoyolabBaseWrapper<WishlogItemWiki>>(url, cancellationToken);
        if (wrapper is null)
        {
            throw new HoyolabException(-1, "Response data is null");
        }
        WishlogItemWiki wiki = wrapper.Data;
        wiki.Language = "zh-cn";
        return wiki;
    }

}
