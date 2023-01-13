using Microsoft.Data.Sqlite;
using Windows.Storage;
using Xunkong.Hoyolab.Wishlog;

namespace Xunkong.Desktop.Services;

internal class BackupService
{


    private readonly JsonSerializerOptions _jsonSerializerOptions;


    public BackupService(JsonSerializerOptions jsonSerializerOptions)
    {
        _jsonSerializerOptions = jsonSerializerOptions;
    }


    public async Task<string> BackupWishlogItemsAsync(int uid, List<WishlogItem> wishlogItems)
    {
        if (wishlogItems == null || !wishlogItems.Any())
        {
            throw new ArgumentNullException(nameof(wishlogItems), "Input wishlog items is null.");
        }
        if (wishlogItems.Any(x => x.Uid != uid))
        {
            throw new ArgumentOutOfRangeException($"Input wishlog items has one that do not match the uid {uid}.");
        }
        var orderedItems = wishlogItems.OrderBy(x => x.Id).ToList();
        var model = new WishlogBackup
        {
            ExportApp = XunkongEnvironment.AppName,
            AppVersion = XunkongEnvironment.AppVersion.ToString(),
            ExportTime = DateTimeOffset.Now,
            Uid = uid,
            WishlogCount = wishlogItems.Count,
            FirstItemTime = wishlogItems.First().Time,
            LastItemTime = wishlogItems.Last().Time,
            WishlogList = orderedItems,
        };
        var dir = Path.Combine(XunkongEnvironment.UserDataPath, @"Backup\Wishlog");
        var file = Path.Combine(dir, $"Wishlog_{uid}_{DateTime.Now:yyyyMMdd_HHmmss}.json");
        Directory.CreateDirectory(dir);
        var json = JsonSerializer.Serialize(model, _jsonSerializerOptions);
        await File.WriteAllTextAsync(file, json);
        return file;
    }


    public async Task<string> BackupWishlogItemsAsync(int uid)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        var items = dapper.Query<WishlogItem>("SELECT * FROM WishlogItem WHERE Uid=@Uid;", new { Uid = uid });
        var model = new WishlogBackup
        {
            ExportApp = XunkongEnvironment.AppName,
            AppVersion = XunkongEnvironment.AppVersion.ToString(),
            ExportTime = DateTimeOffset.Now,
            Uid = uid,
            WishlogCount = items.Count(),
            FirstItemTime = items.FirstOrDefault()?.Time,
            LastItemTime = items.LastOrDefault()?.Time,
            WishlogList = items.ToList(),
        };
        var dir = Path.Combine(XunkongEnvironment.UserDataPath, @"Backup\Wishlog");
        var file = Path.Combine(dir, $"Wishlog_{uid}_{DateTime.Now:yyyyMMdd_HHmmss}.json");
        Directory.CreateDirectory(dir);
        var json = JsonSerializer.Serialize(model, _jsonSerializerOptions);
        await File.WriteAllTextAsync(file, json);
        return file;
    }



    public static string BackupAndCompressDatabase()
    {
        var filePath = Path.Combine(XunkongEnvironment.UserDataPath, $@"Backup\Database\XunkongData_{DateTimeOffset.Now:yyyyMMdd_HHmmss}.db");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        using var backupConnection = new SqliteConnection($"Data Source={filePath};");
        using var cnt = DatabaseProvider.CreateConnection();
        backupConnection.Open();
        cnt.Open();
        cnt.BackupDatabase(backupConnection);
        cnt.Execute("VACUUM;");
        return filePath;
    }


    /// <summary>
    /// 应用关闭时自动备份数据，以7天为周期
    /// </summary>
    public static void AutoBackupDatabase()
    {
        try
        {
            if (!File.Exists(DatabaseProvider.SqlitePath))
            {
                return;
            }
            using var dapper = DatabaseProvider.CreateConnection();
            var operation = dapper.QueryFirstOrDefault<OperationHistory>("SELECT * FROM OperationHistory WHERE Operation='BackupDatabase' ORDER BY Id DESC LIMIT 1;");
            if (operation is not null && DateTimeOffset.Now - operation.Time < TimeSpan.FromDays(7))
            {
                return;
            }
            var dir = Path.Combine(XunkongEnvironment.UserDataPath, @"Backup\Database\");
            Directory.CreateDirectory(dir);
            var filePath = Path.Combine(dir, $@"XunkongData_{DateTimeOffset.Now:yyyyMMdd_HHmmss}.db");
            using var backupConnection = new SqliteConnection($"Data Source={filePath};");
            backupConnection.Open();
            OperationHistory.AddToDatabase("BackupDatabase");
            dapper.Open();
            dapper.BackupDatabase(backupConnection);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "自动备份数据库");
        }
    }




    public static void BackupSetting()
    {
        try
        {
            var settings = typeof(SettingKeys).GetFields()
                                              .Where(x => x.GetCustomAttributes(typeof(BackupAttribute), false).Any())
                                              .Select(x => x.GetRawConstantValue() as string)
                                              .ToList();
            var dic = ApplicationData.Current.LocalSettings.Values;
            var list = new List<object>();
            foreach (var item in settings)
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    var value = dic[item];
                    if (value != null)
                    {
                        list.Add(new { Key = item, Value = value });
                    }
                }
            }
            using var dapper = DatabaseProvider.CreateConnection();
            using var t = dapper.BeginTransaction();
            dapper.Execute("INSERT OR REPLACE INTO Setting (Key, Value) VALUES (@Key, @Value);", list, t);
            t.Commit();
        }
        catch { }
    }



    public static void RestoreSetting()
    {
        try
        {
            var settings = typeof(SettingKeys).GetFields()
                                              .Where(x => x.GetCustomAttributes(typeof(BackupAttribute), false).Any())
                                              .Select(x => x.GetRawConstantValue() as string)
                                              .ToList();
            using var dapper = DatabaseProvider.CreateConnection();
            var list = dapper.Query("SELECT * FROM Setting WHERE Key IN @Keys;", new { Keys = settings });
            var dic = ApplicationData.Current.LocalSettings.Values;
            foreach (var item in list)
            {
                dic[item.Key] = item.Value;
            }
        }
        catch { }
    }



}
