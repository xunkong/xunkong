using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using System.Text.RegularExpressions;
using Xunkong.Hoyolab;
using Xunkong.Hoyolab.Wishlog;
using Xunkong.SnapMetadata;

namespace Xunkong.Desktop.Services;

internal class WishlogService
{


    private readonly WishlogClient _wishlogClient;

    private static readonly string UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    private static readonly string LogFile_Cn = Path.Combine(UserProfile, @"AppData\LocalLow\miHoYo\原神\output_log.txt");

    private static readonly string LogFile_Sea = Path.Combine(UserProfile, @"AppData\LocalLow\miHoYo\Genshin Impact\output_log.txt");


    public WishlogService(WishlogClient wishlogClient)
    {
        _wishlogClient = wishlogClient;
    }



    public static WishType WishTypeToQueryType(WishType type)
    {
        return type switch
        {
            WishType.CharacterEvent_2 => WishType.CharacterEvent,
            _ => type,
        };
    }


    public static RegionType UidToRegionType(int uid)
    {
        return uid.ToString().FirstOrDefault() switch
        {
            '1' => RegionType.cn_gf01,
            '2' => RegionType.cn_gf01,
            '3' => RegionType.cn_gf01,
            '4' => RegionType.cn_gf01,
            '5' => RegionType.cn_qd01,
            '6' => RegionType.os_usa,
            '7' => RegionType.os_euro,
            '8' => RegionType.os_asia,
            '9' => RegionType.os_cht,
            _ => RegionType.None,
        };
    }




    /// <summary>
    /// 获取原神日志中最新的祈愿记录网址
    /// </summary>
    /// <param name="isSea"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    public async Task<string> FindWishlogUrlFromLogFileAsync(bool isSea = false)
    {
        var file = isSea ? LogFile_Sea : LogFile_Cn;
        if (!File.Exists(file))
        {
            file = isSea ? LogFile_Cn : LogFile_Sea;
        }
        if (!File.Exists(file))
        {
            throw new FileNotFoundException("请重新获取祈愿记录网址");
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
            throw new FileNotFoundException("请重新获取祈愿记录网址");
        }
    }





    public static string? FindWishlogUrlFromCloudServer()
    {
        var logFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"GenshinImpactCloudGame\config\logs\MiHoYoSDK.log");
        if (File.Exists(logFile))
        {
            using var fs = File.Open(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var reader = new StreamReader(fs);
            var str = reader.ReadToEnd();
            var matches = Regex.Matches(str, @"https://.+#/log");
            if (matches.Any())
            {
                return matches.Last().Value;
            }
        }
        return null;
    }






    public static void DeleteCacheFile(int server)
    {
        try
        {
            if (GameAccountService.IsGameRunning(server))
            {
                return;
            }
            var exePath = GameAccountService.GetGameExePath(server);
            var folder = Path.GetDirectoryName(exePath);
            string? file = null;
            if (server == 0)
            {
                var matcher = new Matcher();
                matcher.AddInclude(@"YuanShen_Data\webCaches\Cache\Cache_Data\data_2");
                matcher.AddInclude(@"YuanShen_Data\webCaches\*\Cache\Cache_Data\data_2");
                var result = matcher.Execute(new DirectoryInfoWrapper(new FileInfo(exePath).Directory!));
                var files = result.Files.Select(x => Path.Combine(Path.GetDirectoryName(exePath)!, x.Path));
                file = files.OrderByDescending(x => new FileInfo(x).LastWriteTime).FirstOrDefault();
            }
            if (server == 1)
            {
                var matcher = new Matcher();
                matcher.AddInclude(@"GenshinImpact_Data\webCaches\Cache\Cache_Data\data_2");
                matcher.AddInclude(@"GenshinImpact_Data\webCaches\*\Cache\Cache_Data\data_2");
                var result = matcher.Execute(new DirectoryInfoWrapper(new FileInfo(exePath).Directory!));
                var files = result.Files.Select(x => Path.Combine(Path.GetDirectoryName(exePath)!, x.Path));
                file = files.OrderByDescending(x => new FileInfo(x).LastWriteTime).FirstOrDefault();
            }
            if (server == 2)
            {
                file = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"GenshinImpactCloudGame\config\logs\MiHoYoSDK.log");
            }
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
        catch { }
    }



    /// <summary>
    /// 获取祈愿记录网址对应的uid
    /// </summary>
    /// <param name="wishlogUrl"></param>
    /// <returns></returns>
    /// <exception cref="HoyolabException"></exception>
    public async Task<int> GetUidByWishlogUrl(string wishlogUrl)
    {
        var uid = await _wishlogClient.GetUidAsync(wishlogUrl);
        if (uid == 0)
        {
            throw new HoyolabException(-1, "该网址没有对应的祈愿记录");
        }
        using var dapper = DatabaseProvider.CreateConnection();
        var model = new WishlogUrl { Uid = uid, Url = wishlogUrl, DateTime = DateTimeOffset.Now };
        dapper.Execute("INSERT OR REPLACE INTO WishlogUrl (Uid, Url, DateTime) VALUES(@Uid, @Url, @DateTime);", model);
        return uid;
    }



    /// <summary>
    /// 检查指定uid的祈愿记录认证信息是否过期，默认24小时后过期
    /// </summary>
    /// <remarks>
    /// 寻空使用祈愿记录网址确认用户身份，每次从米哈游服务器获取祈愿记录时，会保存此次使用的祈愿记录网址和当前时间，在之后的24小时内认为该认证有效
    /// </remarks>
    /// <param name="uid"></param>
    /// <returns></returns>
    public static bool CheckWishlogUrlExpired(int uid)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        var model = dapper.QueryFirstOrDefault<WishlogUrl>("SELECT * FROM WishlogUrl WHERE Uid=@Uid LIMIT 1;", new { Uid = uid });
        if (model == null)
        {
            return false;
        }
        else
        {
            return model.DateTime + TimeSpan.FromDays(1) > DateTimeOffset.Now;
        }
    }



    /// <summary>
    /// 检查指定uid保存的祈愿记录网址的实时有效性
    /// </summary>
    /// <remarks>
    /// 此有效性不同于祈愿记录认证信息，有效则表示使用此网址能够从米哈游服务器获取祈愿记录
    /// </remarks>
    /// <param name="uid"></param>
    /// <returns></returns>
    public async Task<bool> CheckWishlogUrlTimeoutAsync(int uid)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        var url = dapper.QueryFirstOrDefault<string>("SELECT Url FROM WishlogUrl WHERE Uid=@Uid LIMIT 1;", new { Uid = uid });
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }
        try
        {
            var newUid = await GetUidByWishlogUrl(url);
            if (newUid == uid)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (HoyolabException)
        {
            return false;
        }
    }




    /// <summary>
    /// 根据本地保存的祈愿记录网址，从米哈游服务器获取指定uid的祈愿记录，并返回新增数量
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="onProgressChanged">页数变化时调用</param>
    /// <param name="getAll">是否获取全部数据</param>
    /// <returns>新增祈愿记录的数量</returns>
    /// <exception cref="XunkongException"></exception>
    /// <exception cref="HoyolabException"></exception>
    public async Task<int> GetWishlogByUidAsync(int uid, IProgress<string>? progress = null, bool getAll = false)
    {
        var uidObj = new { Uid = uid };
        using var dapper = DatabaseProvider.CreateConnection();
        var wishlogUrl = dapper.QueryFirstOrDefault<string>("SELECT Url FROM WishlogUrl WHERE Uid=@Uid LIMIT 1;", uidObj);
        if (string.IsNullOrWhiteSpace(wishlogUrl))
        {
            throw new XunkongException($"Cannot find wishlog url of uid {uid}.");
        }
        long lastId = 0;
        if (!getAll)
        {
            lastId = dapper.QueryFirstOrDefault<long>("SELECT Id FROM WishlogItem WHERE Uid=@Uid ORDER BY Id DESC LIMIT 1;", uidObj);
        }
        var progressHanler = new Progress<(WishType QueryType, int Page)>((param) => progress?.Report($"正在获取 {param.QueryType.ToDescription()} 第 {param.Page} 页"));
        var wishlogs = await _wishlogClient.GetAllWishlogAsync(wishlogUrl, lastId, progress: progressHanler);
        progress?.Report("正在写入数据库");
        await Task.Delay(100);
        return InsertWishlogItems(uid, wishlogs);
    }





    /// <summary>
    /// 获取原神日志中祈愿记录网址的祈愿记录，返回新增条数
    /// </summary>
    /// <param name="isSea">首先从外服日志获取</param>
    /// <param name="getAll">是否获取全部数据</param>
    /// <returns></returns>
    /// <exception cref="HoyolabException"></exception>
    public async Task<int> GetWishlogsByLogFileAsync(bool isSea = false, IProgress<string>? progress = null, bool getAll = false)
    {
        var wishlogUrl = await FindWishlogUrlFromLogFileAsync(isSea);
        var uid = await GetUidByWishlogUrl(wishlogUrl);
        var uidObj = new { Uid = uid };
        long lastId = 0;
        using var dapper = DatabaseProvider.CreateConnection();
        if (!getAll)
        {
            lastId = dapper.QueryFirstOrDefault<long>("SELECT Id FROM WishlogItem WHERE Uid=@Uid ORDER BY Id DESC LIMIT 1;", uidObj);
        }
        var progressHanler = new Progress<(WishType QueryType, int Page)>((param) => progress?.Report($"正在获取 {param.QueryType.ToDescription()} 第 {param.Page} 页"));
        var wishlogs = await _wishlogClient.GetAllWishlogAsync(wishlogUrl, lastId, progress: progressHanler);
        progress?.Report("正在写入数据库");
        await Task.Delay(100);
        return InsertWishlogItems(uid, wishlogs);
    }



    public static int InsertWishlogItems(int uid, IEnumerable<WishlogItem> wishlogs)
    {
        var uidObj = new { Uid = uid };
        using var dapper = DatabaseProvider.CreateConnection();
        var oldCount = dapper.QuerySingleOrDefault<int>("SELECT COUNT(*) FROM WishlogItem WHERE Uid=@Uid;", uidObj);
        dapper.Open();
        using var t = dapper.BeginTransaction();
        dapper.Execute("""
            INSERT OR REPLACE INTO WishlogItem (Uid, Id, WishType, Time, Name, Language, ItemType, RankType, QueryType)
            VALUES (@Uid, @Id, @WishType, @Time, @Name, @Language, @ItemType, @RankType, @QueryType);
            """, wishlogs, t);
        t.Commit();
        var newCount = dapper.QuerySingleOrDefault<int>("SELECT COUNT(*) FROM WishlogItem WHERE Uid=@Uid;", uidObj);
        return newCount - oldCount;
    }



    /// <summary>
    /// 获取本地数据库中所有的uid
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<int> GetAllUids()
    {
        using var dapper = DatabaseProvider.CreateConnection();
        return dapper.Query<int>("SELECT DISTINCT Uid FROM WishlogItem;");
    }


    /// <summary>
    /// 获取指定uid的本地祈愿记录数量
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    public static int GetWishlogCount(int uid)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        return dapper.QuerySingleOrDefault<int>("SELECT COUNT(*) FROM WishlogItem WHERE Uid=@Uid;", new { Uid = uid });
    }




    public static List<WishlogItemEx> GetWishlogItemExByUid(int uid)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        var items = dapper.Query<WishlogItemEx>("SELECT * FROM WishlogItem WHERE Uid=@Uid ORDER BY Id;", new { Uid = uid }).ToList();
        var events = XunkongApiService.GetGenshinData<SnapGachaEventInfo>();
        var avatars = XunkongApiService.GetGenshinData<SnapAvatarInfo>().ToDictionary(x => x.Name, x => x.Id);
        var weapons = XunkongApiService.GetGenshinData<SnapWeaponInfo>().GroupBy(x => x.Name).ToDictionary(x => x.First().Name, x => x.First().Id);
        var groups = items.GroupBy(x => x.QueryType).ToList();

        foreach (var group in groups)
        {
            var groupList = group.OrderBy(x => x.Id).ToList();

            var queryType = group.Key;
            if (queryType is WishType.Novice or WishType.Permanent)
            {
                var eventName = queryType switch
                {
                    WishType.Novice => "新手祈愿",
                    WishType.Permanent => "常驻祈愿",
                    _ => "",
                };
                groupList.ForEach(x => x.WishEventName = eventName);
            }

            int guaranteeIndex = 0;
            for (int i = 0; i < groupList.Count; i++)
            {
                var item = groupList[i];
                guaranteeIndex++;
                item.GuaranteeIndex = guaranteeIndex;
                if (item.RankType == 5)
                {
                    guaranteeIndex = 0;
                }
            }

            if (queryType is WishType.CharacterEvent or WishType.WeaponEvent)
            {
                int startIndex = 0, endIndex = 0;
                foreach (var e in events.Where(x => x.QueryType == ((int)queryType)))
                {
                    var index = endIndex == 0 ? 0 : endIndex - 1;
                    if (groupList[index].Time < e.From)
                    {
                        startIndex = endIndex;
                    }
                    for (int i = startIndex; i < groupList.Count; i++)
                    {
                        var item = groupList[i];
                        if (item.Time < e.From)
                        {
                            continue;
                        }
                        if (item.Time > e.To)
                        {
                            endIndex = i;
                            break;
                        }
                        if (((int)item.WishType) == e.Type)
                        {
                            item.Version = e.Version;
                            item.WishEventName = e.Name;
                        }
                        if (((int)item.QueryType) == e.QueryType)
                        {
                            int id = 0;
                            if (queryType == WishType.CharacterEvent)
                            {
                                id = avatars.GetValueOrDefault(item.Name);
                            }
                            if (queryType == WishType.WeaponEvent)
                            {
                                id = weapons.GetValueOrDefault(item.Name);
                            }
                            if (item.RankType == 5 && e.UpOrangeList.Contains(id))
                            {
                                item.IsUp = true;
                            }
                            if (item.RankType == 4 && e.UpOrangeList.Contains(id))
                            {
                                item.IsUp = true;
                            }
                        }
                    }
                }
            }
        }

        bool waile = false; // 歪了
        foreach (var item in items.Where(x => x.RankType == 5 && x.QueryType == WishType.CharacterEvent))
        {
            if (item.IsUp)
            {
                if (waile)
                {
                    item.IsDabaodi = true;
                    waile = false;
                }
            }
            else
            {
                waile = true;
            }
        }

        waile = false;
        foreach (var item in items.Where(x => x.RankType == 4 && x.QueryType == WishType.CharacterEvent))
        {
            if (item.IsUp)
            {
                if (waile)
                {
                    item.IsDabaodi = true;
                    waile = false;
                }
            }
            else
            {
                waile = true;
            }
        }

        waile = false;
        foreach (var item in items.Where(x => x.RankType == 5 && x.QueryType == WishType.WeaponEvent))
        {
            if (item.IsUp)
            {
                if (waile)
                {
                    item.IsDabaodi = true;
                    waile = false;
                }
            }
            else
            {
                waile = true;
            }
        }

        waile = false;
        foreach (var item in items.Where(x => x.RankType == 4 && x.QueryType == WishType.CharacterEvent))
        {
            if (item.IsUp)
            {
                if (waile)
                {
                    item.IsDabaodi = true;
                    waile = false;
                }
            }
            else
            {
                waile = true;
            }
        }

        return items;
    }



}
