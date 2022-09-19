using CommunityToolkit.WinUI.Notifications;
using Scighost.WinUILib.Cache;
using System.Diagnostics;
using System.Threading;
using Windows.UI.Notifications;
using Xunkong.GenshinData.Achievement;
using Xunkong.GenshinData.Character;
using Xunkong.GenshinData.Weapon;

namespace Xunkong.Desktop.Services;

internal static class PreCacheService
{

    const string tag = "PreCacheAllFiles";
    const string group = "precache";

    private static FileCache _fileCache;

    private static CancellationTokenSource _cancellationTokenSource;


    public static bool IsRunning { get; private set; }

    /// <summary>
    /// 开始预下载
    /// </summary>
    public static void StartPreCache()
    {
        if (IsRunning)
        {
            return;
        }
        _cancellationTokenSource = new CancellationTokenSource();
        var thread = new Thread(new ThreadStart(() => PreCacheAllFilesAsync(_cancellationTokenSource.Token)));
        thread.Start();
    }


    /// <summary>
    /// 停止预下载
    /// </summary>
    public static void StopPreCache()
    {
        if (IsRunning)
        {
            _cancellationTokenSource.Cancel();
        }
    }


    private static async void PreCacheAllFilesAsync(CancellationToken token = default)
    {
        try
        {
            IsRunning = true;
            var folder = (await ImageCache.Instance.GetCacheFolderAsync()).Path;
            if (Directory.Exists(folder))
            {
                foreach (var file in new DirectoryInfo(folder).GetFiles())
                {
                    if (file.Length == 0)
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, "删除未缓存的文件");
                        }
                    }
                }
            }

            var list = GetAllImages();

            if (list.Any())
            {
                if (_fileCache is null)
                {
                    _fileCache = new FileCache();
                    _fileCache.Initialize(await ImageCache.Instance.GetCacheFolderAsync());
                }

                var sw = Stopwatch.StartNew();
                long lastMs = 0;
                int total = list.Count;
                ulong downloadSize = 0;
                int downloadCount = 0;
                var manager = ToastNotificationManager.CreateToastNotifier();
                var toast = SendToast(manager, total);

                uint index = 0;
                await Parallel.ForEachAsync(list, new ParallelOptions { CancellationToken = token, MaxDegreeOfParallelism = Environment.ProcessorCount * 4 }, async (url, t) =>
                {
                    var file = await _fileCache.GetFromCacheAsync(new Uri(url), cancellationToken: t);
                    ulong size = 0;
                    if (file is not null)
                    {
                        var prop = await file.GetBasicPropertiesAsync();
                        size = prop.Size;
                    }
                    lock (list)
                    {
                        index++;
                        if (size > 0)
                        {
                            downloadSize += size;
                            downloadCount++;
                        }
                        var ems = sw.ElapsedMilliseconds;
                        if (ems - lastMs > 33)
                        {
                            lastMs = ems;
                            UpdateToast(manager, total, index, downloadSize);
                        }
                    }
                });

                sw.Stop();

                manager.Hide(toast);

                await ToastProvider.SendAsync("预缓存完成", $"下载文件 {downloadCount} 个，数据量 {(double)downloadSize / (1 << 20):F2} MB，耗时 {sw.Elapsed.TotalSeconds:N0} 秒。");

            }
            else
            {
                NotificationProvider.Success("所有图片均已缓存");
            }
        }
        catch (TaskCanceledException)
        {
            Logger.Info("缓存任务已取消");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "缓存所有图片");
        }
        finally
        {
            IsRunning = false;
        }
    }



    private static List<string> GetAllImages()
    {
        var list = new List<string?>();
        var obj = new List<string>();
        using var liteDb = DatabaseProvider.CreateLiteDB();
        var characters = liteDb.GetCollection<CharacterInfo>().FindAll().ToList();
        list.AddRange(characters.Select(x => x.FaceIcon));
        list.AddRange(characters.Select(x => x.SideIcon));
        list.AddRange(characters.Select(x => x.Card));
        list.AddRange(characters.Select(x => x.GachaCard));
        list.AddRange(characters.Select(x => x.GachaSplash));
        list.AddRange(characters.Select(x => x.Food?.Icon));
        list.AddRange(characters.Select(x => x.NameCard?.Icon));
        list.AddRange(characters.Select(x => x.NameCard?.ProfileImage));
        list.AddRange(characters.Select(x => x.NameCard?.GalleryBackground));
        list.AddRange(characters.SelectMany(x => x.Talents?.Select(x => x.Icon) ?? obj));
        list.AddRange(characters.SelectMany(x => x.Constellations?.Select(x => x.Icon) ?? obj));
        list.AddRange(characters.SelectMany(x => x.Promotions?.SelectMany(x => x.CostItems?.Select(x => x.Item?.Icon) ?? obj) ?? obj));
        list.AddRange(characters.SelectMany(x => x.Talents?.SelectMany(x => x.Levels?.SelectMany(x => x.CostItems?.Select(x => x.Item?.Icon) ?? obj) ?? obj) ?? obj));
        list.AddRange(characters.SelectMany(x => x.Outfits?.Select(x => x.FaceIcon)?.Where(x => x?.StartsWith("http") ?? false) ?? obj));
        list.AddRange(characters.SelectMany(x => x.Outfits?.Select(x => x.SideIcon)?.Where(x => x?.StartsWith("http") ?? false) ?? obj));
        list.AddRange(characters.SelectMany(x => x.Outfits?.Select(x => x.Card)?.Where(x => x?.StartsWith("http") ?? false) ?? obj));
        list.AddRange(characters.SelectMany(x => x.Outfits?.Select(x => x.GachaSplash)?.Where(x => x?.StartsWith("http") ?? false) ?? obj));

        // 语音数据，约 15000 个文件，1.7 GB
        //list.AddRange(characters.SelectMany(x => x.Voices?.Select(x => x.Chinese) ?? obj));
        //list.AddRange(characters.SelectMany(x => x.Voices?.Select(x => x.English) ?? obj));
        //list.AddRange(characters.SelectMany(x => x.Voices?.Select(x => x.Japanese) ?? obj));
        //list.AddRange(characters.SelectMany(x => x.Voices?.Select(x => x.Korean) ?? obj));

        var weapons = liteDb.GetCollection<WeaponInfo>().FindAll().ToList();
        list.AddRange(weapons.Select(x => x.Icon));
        list.AddRange(weapons.Select(x => x.AwakenIcon));
        list.AddRange(weapons.Select(x => x.GachaIcon));
        list.AddRange(weapons.SelectMany(x => x.Promotions?.SelectMany(x => x.CostItems?.Select(x => x.Item.Icon) ?? obj) ?? obj));

        var goals = liteDb.GetCollection<AchievementGoal>().FindAll().ToList();
        list.AddRange(goals.Select(x => x.IconPath));
        list.AddRange(goals.Select(x => x.SmallIcon));
        list.AddRange(goals.Select(x => x.RewardNameCard?.Icon));
        list.AddRange(goals.Select(x => x.RewardNameCard?.ProfileImage));
        list.AddRange(goals.Select(x => x.RewardNameCard?.GalleryBackground));

        return list.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().Where(x => !File.Exists(ImageCache.Instance.GetCacheFilePath(new Uri(x!)))).ToList()!;
    }



    private static ToastNotification SendToast(ToastNotifier manager, int total)
    {
        var content = new ToastContentBuilder().AddText("下载图片中。。。").AddVisualChild(new AdaptiveProgressBar()
        {
            Title = new BindableString("progressTitle"),
            Value = new BindableProgressBarValue("progressValue"),
            ValueStringOverride = new BindableString("progressValueString"),
            Status = new BindableString("progressStatus")
        }).AddButton("取消", ToastActivationType.Foreground, "CancelPreCacheAllFiles")
        .AddToastActivationInfo("PreCacheAllFiles", ToastActivationType.Background).GetToastContent();

        var toast = new ToastNotification(content.GetXml());
        toast.Tag = tag;
        toast.Group = group;
        toast.Data = new NotificationData();
        toast.Data.Values["progressTitle"] = "0 MB";
        toast.Data.Values["progressValue"] = "0";
        toast.Data.Values["progressValueString"] = "0%";
        toast.Data.Values["progressStatus"] = $"0 / {total}";
        toast.Data.SequenceNumber = 0;
        manager.Show(toast);
        return toast;
    }


    private static void UpdateToast(ToastNotifier manager, int total, uint index, ulong totalSize)
    {
        var data = new NotificationData { SequenceNumber = index };
        data.Values["progressTitle"] = $"{(double)totalSize / (1 << 20):F2} MB";
        data.Values["progressValue"] = $"{(double)index / total}";
        data.Values["progressValueString"] = $"{(double)index / total:P1}";
        data.Values["progressStatus"] = $"{index} / {total}";
        manager.Update(data, tag, group);
    }



}
