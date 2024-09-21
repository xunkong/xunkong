using System.Collections.Concurrent;
using System.Net.Http;
using System.Text.Json.Nodes;
using Xunkong.ApiClient;
using Xunkong.ApiClient.GenshinData;
using Xunkong.ApiClient.Xunkong;
using Xunkong.GenshinData.Achievement;
using Xunkong.GenshinData.Character;
using Xunkong.GenshinData.Material;
using Xunkong.GenshinData.Text;
using Xunkong.GenshinData.Weapon;
using Xunkong.Hoyolab.Wishlog;
using Xunkong.SnapMetadata;

namespace Xunkong.Desktop.Services;

internal class XunkongApiService
{

    private readonly HttpClient _httpClient;

    private readonly XunkongApiClient _xunkongClient;

    private readonly WishlogService _wishlogService;

    private readonly BackupService _backupService;

    private readonly SnapMetadataClient _snapMetadataClient;

    public XunkongApiService(HttpClient httpClient, XunkongApiClient xunkongClient, WishlogService wishlogService, BackupService backupService, SnapMetadataClient snapMetadataClient)
    {
        _httpClient = httpClient;
        _xunkongClient = xunkongClient;
        _wishlogService = wishlogService;
        _backupService = backupService;
        _snapMetadataClient = snapMetadataClient;
    }



    public static string? WallpaperRequestFormat { get; set; }


    static XunkongApiService()
    {
        LiteDB.BsonMapper.Global.Entity<TextMapItem>().Id(x => x.ItemId);
        WallpaperRequestFormat = AppSetting.GetValue<string>(SettingKeys.WallpaperRequestFormat);
    }




    #region Desktop


    public async Task<List<InfoBarContent>> GetInfoBarContentListAsync()
    {
        return await _xunkongClient.GetInfoBarContentListAsync(XunkongEnvironment.Channel, XunkongEnvironment.AppVersion);
    }


    #endregion



    #region Wishlog Backup



    private async Task<string> CheckWishlogUrlAsync(int uid, IProgress<string>? progress = null)
    {
        progress?.Report("检查祈愿记录网址的有效性");
        using var dapper = DatabaseProvider.CreateConnection();
        var auth = dapper.QueryFirstOrDefault<WishlogUrl>("SELECT * FROM WishlogUrl WHERE Uid=@Uid;", new { Uid = uid });
        if (auth is not null)
        {
            if (DateTimeOffset.Now < auth.DateTime + TimeSpan.FromDays(1))
            {
                return auth.Url;
            }
        }
        var isSea = uid.ToString().FirstOrDefault() > '5';
        var url = await _wishlogService.FindWishlogUrlFromLogFileAsync(isSea);
        var newUid = await _wishlogService.GetUidByWishlogUrl(url);
        if (uid == newUid)
        {
            return url;
        }
        throw new XunkongException($"Wishlog url of uid {uid} is expired or not found.");
    }




    public async Task<WishlogBackupResult> GetWishlogBackupLastItemAsync(int uid, IProgress<string>? progress = null)
    {
        var url = await CheckWishlogUrlAsync(uid);
        var model = new WishlogBackupRequest { Uid = uid, Url = url };
        progress?.Report("查询云端的最新记录");
        var result = await _xunkongClient.GetWishlogLastItemFromCloudAsync(model);
        progress?.Report($"云端现有 {result.CurrentCount} 条记录");
        return result;
    }



    public async Task<WishlogBackupResult> GetWishlogBackupListAsync(int uid, IProgress<string>? progress = null, bool getAll = false)
    {
        var url = await CheckWishlogUrlAsync(uid, progress);
        using var dapper = DatabaseProvider.CreateConnection();
        var uidObj = new { Uid = uid };
        long lastId = 0;
        if (!getAll)
        {
            lastId = dapper.QueryFirstOrDefault<long>("SELECT Id FROM WishlogItem WHERE Uid=@Uid ORDER BY Id DESC;", uidObj);
        }
        var model = new WishlogBackupRequest { Uid = uid, Url = url, LastId = lastId };
        progress?.Report("下载祈愿记录");
        var result = await _xunkongClient.GetWishlogListFromCloudAsync(model);
        var oldCount = dapper.QuerySingleOrDefault<int>("SELECT COUNT(*) FROM WishlogItem WHERE Uid=@Uid;", uidObj);
        if (result.List?.Any() ?? false)
        {
            dapper.Open();
            using var t = dapper.BeginTransaction();
            progress?.Report("写入数据库");
            dapper.Execute("""
                 INSERT OR REPLACE INTO WishlogItem (Uid, Id, WishType, Time, Name, Language, ItemType, RankType, QueryType)
                 VALUES (@Uid, @Id, @WishType, @Time, @Name, @Language, @ItemType, @RankType, @QueryType);
                 """, result.List, t);
            t.Commit();
        }
        var newCount = dapper.QuerySingleOrDefault<int>("SELECT COUNT(*) FROM WishlogItem WHERE Uid=@Uid;", uidObj);
        result.PutCount = newCount - oldCount;
        progress?.Report($"本地新增 {result.PutCount} 条，本地现有 {newCount} 条，云端现有 {result.CurrentCount} 条");
        result.List = null;
        return result;
    }



    public async Task<string> GetWishlogAndBackupToLoalAsync(int uid, IProgress<string>? progress = null)
    {
        var url = await CheckWishlogUrlAsync(uid, progress);
        var model = new WishlogBackupRequest { Uid = uid, Url = url, LastId = 0 };
        progress?.Report("备份云端祈愿记录到本地");
        var result = await _xunkongClient.GetWishlogListFromCloudAsync(model);
        var backupFile = await _backupService.BackupWishlogItemsAsync(result.Uid, result.List!);
        return backupFile;
    }



    public async Task<WishlogBackupResult> PutWishlogListAsync(int uid, IProgress<string>? progress = null, bool putAll = false)
    {
        var url = await CheckWishlogUrlAsync(uid, progress);
        var model = new WishlogBackupRequest { Uid = uid, Url = url };
        long lastId = 0;
        WishlogBackupResult result = new(uid, 0, 0, 0, 0, null);
        if (!putAll)
        {
            progress?.Report("查询云端的最新记录");
            result = await _xunkongClient.GetWishlogLastItemFromCloudAsync(model);
            lastId = result.List?.LastOrDefault()?.Id ?? 0;
        }
        using var dapper = DatabaseProvider.CreateConnection();
        var list = dapper.Query<WishlogItem>("SELECT * FROM WishlogItem WHERE Uid=@Uid AND Id>@Id;", new { Uid = uid, Id = lastId });
        if (list.Any())
        {
            progress?.Report("上传祈愿记录");
            int addCount = 0;
            foreach (var chunk in list.Chunk(10000))
            {
                model.List = list;
                result = await _xunkongClient.PutWishlogListToCloudAsync(model);
                addCount += result.PutCount;
            }
        }
        progress?.Report($"上传 {result.PutCount} 条，云端现有 {result.CurrentCount} 条");
        return result;
    }


    public async Task<WishlogBackupResult> DeleteWishlogBackupAsync(int uid, IProgress<string>? progress = null)
    {
        var url = await CheckWishlogUrlAsync(uid, progress);
        var model = new WishlogBackupRequest { Uid = uid, Url = url };
        progress?.Report("发送删除请求");
        var result = await _xunkongClient.DeleteWishlogInCloudAsync(model);
        progress?.Report($"已删除 {result.Uid} 祈愿记录 {result.DeleteCount} 条");
        return result;
    }



    #endregion



    #region Genshin Data



    private static readonly ConcurrentDictionary<string, object> GenshinDataDic = new();



    public async Task GetAllGenshinDataFromServerAsync(bool force = false)
    {
        await GetSnapMetadataAsync(force);
        //var data = await _xunkongClient.GetAllGenshinDataAsync();
        //SaveGenshinData(data);
    }


    private async Task GetSnapMetadataAsync(bool force = false)
    {
        var meta = await _snapMetadataClient.GetSnapMetaAsync();
        // Achievement
        string? hash = AppSetting.GetValue<string>(nameof(meta.Achievement));
        if (force || meta.Achievement != hash)
        {
            var data = await _snapMetadataClient.GetAchievementItemsAsync();
            SaveSnapMetadata(data);
            AppSetting.SetValue(nameof(meta.Achievement), meta.Achievement);
        }
        // AchievementGoal
        hash = AppSetting.GetValue<string>(nameof(meta.AchievementGoal));
        if (force || meta.AchievementGoal != hash)
        {
            var data = await _snapMetadataClient.GetAchievementGoalsAsync();
            if (data.FirstOrDefault(x => x.Id == 0) is SnapAchievementGoal goal)
            {
                goal.Id = 10001;
            }
            SaveSnapMetadata(data);
            AppSetting.SetValue(nameof(meta.AchievementGoal), meta.AchievementGoal);
        }
        // Avatars
        foreach (var item in meta.Avatars)
        {
            hash = AppSetting.GetValue<string>(item.Key);
            if (force || item.Value as string != hash)
            {
                var list = new List<SnapAvatarInfo>(meta.Avatars.Count);
                await Parallel.ForEachAsync(meta.Avatars, async (item, _) =>
                {
                    if (item.Key.StartsWith("Avatar/"))
                    {
                        var info = await _snapMetadataClient.GetAvatarInfoAsync(item.Key);
                        lock (list)
                        {
                            list.Add(info);
                        }
                        AppSetting.SetValue(item.Key, item.Value);
                    }
                });
                SaveSnapMetadata(list);
                break;
            }
        }
        // Weapon
        hash = AppSetting.GetValue<string>(nameof(meta.Weapon));
        if (force || meta.Weapon != hash)
        {
            var data = await _snapMetadataClient.GetWeaponInfosAsync();
            SaveSnapMetadata(data);
            AppSetting.SetValue(nameof(meta.Weapon), meta.Weapon);
        }
        // GachaEvent
        hash = AppSetting.GetValue<string>(nameof(meta.GachaEvent));
        if (force || meta.GachaEvent != hash)
        {
            var data = await _snapMetadataClient.GetGachaEventInfosAsync();
            SaveSnapMetadata(data);
            AppSetting.SetValue(nameof(meta.GachaEvent), meta.GachaEvent);
        }
        // DisplayItem
        hash = AppSetting.GetValue<string>(nameof(meta.DisplayItem));
        if (force || meta.DisplayItem != hash)
        {
            var data = await _snapMetadataClient.GetDisplayItemsAsync();
            SaveSnapMetadata(data);
            AppSetting.SetValue(nameof(meta.DisplayItem), meta.DisplayItem);
        }
        // AvatarPromote & WeaponPromote
        hash = AppSetting.GetValue<string>(nameof(meta.AvatarPromote));
        string? hash2 = AppSetting.GetValue<string>(nameof(meta.WeaponPromote));
        if (force || meta.AvatarPromote != hash || meta.WeaponPromote != hash2)
        {
            var data = await _snapMetadataClient.GetPromotesAsync();
            SaveSnapMetadata(data);
            AppSetting.SetValue(nameof(meta.AvatarPromote), meta.AvatarPromote);
            AppSetting.SetValue(nameof(meta.WeaponPromote), meta.WeaponPromote);
        }
        // Material
        hash = AppSetting.GetValue<string>(nameof(meta.Material));
        if (force || meta.Material != hash)
        {
            var data = await _snapMetadataClient.GetMaterialsAsync();
            SaveSnapMetadata(data);
            AppSetting.SetValue(nameof(meta.Material), meta.Material);
        }
    }


    private static void SaveGenshinData(AllGenshinData data)
    {
        using var liteDb = DatabaseProvider.CreateGenshinDataDb();

        GenshinDataDic[nameof(CharacterInfo)] = data.Characters;
        var col1 = liteDb.GetCollection<CharacterInfo>();
        col1.DeleteAll();
        col1.InsertBulk(data.Characters);

        GenshinDataDic[nameof(WeaponInfo)] = data.Weapons;
        var col2 = liteDb.GetCollection<WeaponInfo>();
        col2.DeleteAll();
        col2.InsertBulk(data.Weapons);

        GenshinDataDic[nameof(WishEventInfo)] = data.WishEvents;
        var col3 = liteDb.GetCollection<WishEventInfo>();
        col3.DeleteAll();
        col3.InsertBulk(data.WishEvents);

        GenshinDataDic[nameof(AchievementItem)] = data.Achievement.Items;
        var col4 = liteDb.GetCollection<AchievementItem>();
        col4.DeleteAll();
        col4.InsertBulk(data.Achievement.Items);

        GenshinDataDic[nameof(AchievementGoal)] = data.Achievement.Goals;
        var col5 = liteDb.GetCollection<AchievementGoal>();
        col5.DeleteAll();
        col5.InsertBulk(data.Achievement.Goals);

        GenshinDataDic[nameof(MaterialItem)] = data.Materials;
        var col6 = liteDb.GetCollection<MaterialItem>();
        col6.DeleteAll();
        col6.InsertBulk(data.Materials);

        GenshinDataDic[nameof(TextMapItem)] = data.TextMaps;
        var col7 = liteDb.GetCollection<TextMapItem>();
        col7.DeleteAll();
        col7.InsertBulk(data.TextMaps);
    }


    private static void SaveSnapMetadata<T>(List<T> data)
    {
        using var liteDb = DatabaseProvider.CreateGenshinDataDb();
        GenshinDataDic[typeof(T).Name] = data;
        var col1 = liteDb.GetCollection<T>();
        col1.DeleteAll();
        col1.InsertBulk(data);
    }


    public static List<T> GetGenshinData<T>()
    {
        if (GenshinDataDic.TryGetValue(typeof(T).Name, out var value))
        {
            if (value is List<T> t)
            {
                return t;
            }
        }
        using var liteDb = DatabaseProvider.CreateGenshinDataDb();
        var list = liteDb.GetCollection<T>().FindAll().ToList();
        GenshinDataDic[typeof(T).Name] = list;
        return list;
    }



    #endregion



    #region Genshin Wallpaper


    private WallpaperInfoEx GetWallpaperInfoEx(WallpaperInfo info)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        using var t = dapper.BeginTransaction();
        dapper.Execute("""
            INSERT OR REPLACE INTO WallpaperInfo (Id, Enable, Title, Author, Description, FileName, Tags, Url, Source, Rating, RatingCount)
            VALUES (@Id, @Enable, @Title, @Author, @Description, @FileName, @Tags, @Url, @Source, @Rating, @RatingCount);
            """, info, t);
        var Time = DateTimeOffset.Now;
        dapper.Execute("INSERT INTO WallpaperHistory (Time, WallpaperId) VALUES (@Time, @WallpaperId);", new { Time, WallpaperId = info.Id }, t);
        t.Commit();
        var ex = WallpaperInfoEx.FromWallpaper(info);
        ex.MyRating = dapper.QueryFirstOrDefault<int>("SELECT IFNULL((SELECT Rating FROM WallpaperRating WHERE WallpaperId = @Id LIMIT 1), -1);", info);
        return ex;
    }


    private List<WallpaperInfoEx> GetWallpaperInfoEx(List<WallpaperInfo> infos)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        using var t = dapper.BeginTransaction();
        dapper.Execute("""
            INSERT OR REPLACE INTO WallpaperInfo (Id, Enable, Title, Author, Description, FileName, Tags, Url, Source, Rating, RatingCount)
            VALUES (@Id, @Enable, @Title, @Author, @Description, @FileName, @Tags, @Url, @Source, @Rating, @RatingCount);
            """, infos, t);
        var Time = DateTimeOffset.Now;
        dapper.Execute("INSERT INTO WallpaperHistory (Time, WallpaperId) VALUES (@Time, @WallpaperId);", infos.Select(x => new { Time, WallpaperId = x.Id }), t);
        t.Commit();
        var ex = infos.Select(WallpaperInfoEx.FromWallpaper).ToList();
        foreach (var item in ex)
        {
            item.MyRating = dapper.QueryFirstOrDefault<int>("SELECT IFNULL((SELECT Rating FROM WallpaperRating WHERE WallpaperId = @Id LIMIT 1), -1);", item);
        }
        return ex;
    }


    public async Task<WallpaperInfoEx> GetWallpaperByIdAsync(int id)
    {
        var info = await _xunkongClient.GetWallpaperByIdAsync(id, WallpaperRequestFormat);
        return GetWallpaperInfoEx(info);

    }



    public async Task<WallpaperInfoEx> GetRandomWallpaperAsync()
    {
        var info = await _xunkongClient.GetRandomWallpaperAsync(WallpaperRequestFormat);
        return GetWallpaperInfoEx(info);
    }


    public async Task<WallpaperInfoEx> GetNextWallpaperAsync(int lastId = 0)
    {
        var info = await _xunkongClient.GetNextWallpaperAsync(lastId, WallpaperRequestFormat);
        return GetWallpaperInfoEx(info);
    }


    public async Task<List<WallpaperInfoEx>> GetWallpaperInfoListAsync(int size)
    {
        var infos = await _xunkongClient.GetWallpaperListAsync(size, WallpaperRequestFormat);
        return GetWallpaperInfoEx(infos);
    }


    public WallpaperInfoEx? GetPreparedWallpaper()
    {
        try
        {
            var url = AppSetting.GetValue<string>(SettingKeys.LauncherBackgroundUrl);
            if (!string.IsNullOrWhiteSpace(url))
            {
                return new WallpaperInfoEx
                {
                    Url = url,
                    FileName = Path.GetFileName(url),
                    Enable = true,
                    Source = url,
                };
            }
            else
            {
                return null;
            }
        }
        catch
        {
            return null;
        }
    }



    public async Task PrepareNextWallpaperAsync()
    {
        try
        {
            var url = await GetHypBackgroundAsync();
            if (!string.IsNullOrWhiteSpace(url))
            {
                var file = XunkongCache.Instance.GetCacheFilePath(new Uri(url));
                if (!File.Exists(file))
                {
                    var bytes = await _httpClient.GetByteArrayAsync(url);
                    Directory.CreateDirectory(Path.GetDirectoryName(file)!);
                    await File.WriteAllBytesAsync(file, bytes);
                }
                AppSetting.SetValue(SettingKeys.LauncherBackgroundUrl, url);
            }
        }
        catch { }
    }



    private async Task<string?> GetHypBackgroundAsync()
    {
        const string url = "https://hyp-api.mihoyo.com/hyp/hyp-connect/api/getGames?launcher_id=jGHBHlcOq1&language=zh-cn";
        string str = await _httpClient.GetStringAsync(url);
        var node = JsonNode.Parse(str);
        if (node?["data"]?["games"] is JsonArray array)
        {
            foreach (var item in array)
            {
                if (item?["biz"]?.ToString() is "hk4e_cn")
                {
                    return item["display"]?["background"]?["url"]?.ToString();
                }
            }
        }
        return null;
    }




    public static void SaveWallpaperRating(WallpaperInfoEx info)
    {
        try
        {
            using var dapper = DatabaseProvider.CreateConnection();
            dapper.Execute("INSERT OR REPLACE INTO WallpaperRating (WallpaperId, Time, Rating, Uploaded) VALUES (@Id, @Now, @MyRating, FALSE);", new { info.Id, DateTimeOffset.Now, info.MyRating });
            OperationHistory.AddToDatabase("RatingWallpaper", info.Id.ToString(), info.MyRating.ToString());
            Logger.TrackEvent("RatingWallpaper", "Id", info.Id.ToString(), "Rating", info.MyRating.ToString());
        }
        catch { }
    }



    public async void UploadWallpaperRatingAsync()
    {
        try
        {
            await Task.Delay(5000);
            using var dapper = DatabaseProvider.CreateConnection();
            if (dapper.QueryFirstOrDefault<bool>("SELECT EXISTS(SELECT * FROM WallpaperRating WHERE Time < @Time AND Uploaded = FALSE);", new { Time = DateTimeOffset.Now.AddHours(-2) }))
            {
                var ratings = dapper.Query<WallpaperRating>("SELECT * FROM WallpaperRating WHERE Uploaded = FALSE;");
                foreach (var item in ratings)
                {
                    item.DeviceId = XunkongEnvironment.DeviceId;
                }
                await _xunkongClient.UploadWallpaperRatingAsync(ratings);
                dapper.Execute("UPDATE WallpaperRating SET Uploaded=TRUE WHERE WallpaperId IN @Ids;", new { Ids = ratings.Select(x => x.WallpaperId).ToList() });
                OperationHistory.AddToDatabase("UploadWallpaperRating");
                Logger.TrackEvent("UploadWallpaperRating");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }



    public static void ChangeWallpaperFileExtension(string? format)
    {
        try
        {
            format = format switch
            {
                "avif" or "jpg" or "png" or "webp" => format,
                _ => "avif",
            };
            using var dapper = DatabaseProvider.CreateConnection();
            using var t = dapper.BeginTransaction();
            dapper.Execute($"""
                UPDATE WallpaperInfo SET FileName=(SUBSTR(FileName, 1, LENGTH(FileName) - 3) || @format) WHERE Url LIKE '%xunkong.cc%' AND FileName LIKE '%.jpg';
                UPDATE WallpaperInfo SET FileName=(SUBSTR(FileName, 1, LENGTH(FileName) - 3) || @format) WHERE Url LIKE '%xunkong.cc%' AND FileName LIKE '%.png';
                UPDATE WallpaperInfo SET FileName=(SUBSTR(FileName, 1, LENGTH(FileName) - 4) || @format) WHERE Url LIKE '%xunkong.cc%' AND FileName LIKE '%.webp';
                UPDATE WallpaperInfo SET FileName=(SUBSTR(FileName, 1, LENGTH(FileName) - 4) || @format) WHERE Url LIKE '%xunkong.cc%' AND FileName LIKE '%.avif';

                UPDATE WallpaperInfo SET Url=REPLACE(Url, '!jpg', '') WHERE Url LIKE '%xunkong.cc%!jpg';
                UPDATE WallpaperInfo SET Url=REPLACE(Url, '!png', '') WHERE Url LIKE '%xunkong.cc%!png';
                UPDATE WallpaperInfo SET Url=REPLACE(Url, '!webp', '') WHERE Url LIKE '%xunkong.cc%!webp';
                UPDATE WallpaperInfo SET Url=REPLACE(Url, '.webp', '.avif') WHERE Url LIKE '%xunkong.cc%.webp';
                """, new { format }, t);
            if (format is "jpg" or "png" or "webp")
            {
                dapper.Execute("""
                    UPDATE WallpaperInfo SET Url=(Url || '!' || @format) WHERE Url LIKE '%xunkong.cc%.avif';
                    """, new { format }, t);
            }
            t.Commit();
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }



    public async Task<int> RefreshLocalWallpaperInfosAsync()
    {
        using var dapper = DatabaseProvider.CreateConnection();
        var ids = dapper.Query<int>("SELECT Id FROM WallpaperInfo;").ToList();
        var list = await _xunkongClient.GetWallpaperInfosByIdsAsnyc(ids, WallpaperRequestFormat);
        using var t = dapper.BeginTransaction();
        dapper.Execute("""
            INSERT OR REPLACE INTO WallpaperInfo (Id, Enable, Title, Author, Description, FileName, Tags, Url, Source, Rating, RatingCount)
            VALUES (@Id, @Enable, @Title, @Author, @Description, @FileName, @Tags, @Url, @Source, @Rating, @RatingCount);
            """, list, t);
        t.Commit();
        return list?.Count ?? 0;
    }



    #endregion



}
