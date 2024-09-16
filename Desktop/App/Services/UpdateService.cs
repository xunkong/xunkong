using CommunityToolkit.WinUI.Notifications;
using Windows.Foundation;
using Windows.Services.Store;
using Windows.UI.Notifications;

namespace Xunkong.Desktop.Services;

internal class UpdateService
{

    private readonly GithubService _githubService;

    public UpdateService(GithubService githubService)
    {
        _githubService = githubService;
    }


    /// <summary>
    /// 检查更新
    /// </summary>
    /// <returns></returns>
    public async Task<(GithubService.GithubRelease? Github, IReadOnlyList<StorePackageUpdate>? Store)> CheckUpdateAsync(bool throwError = false)
    {

        if (XunkongEnvironment.IsStoreVersion)
        {
            // 预览版检查更新
            if (AppSetting.GetValue<bool>(SettingKeys.EnablePrerelease))
            {
                try
                {
                    var release = await _githubService.GetLatestReleaseAsync();
                    if (Version.TryParse(release?.TagName, out var version))
                    {
                        if (version > XunkongEnvironment.AppVersion && release.Prerelease)
                        {
                            return (release, null);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (throwError)
                    {
                        throw;
                    }
                    Logger.Error(ex, "检查预览版更新");
                }
            }
            // 商店版检查更新
            var context = StoreContext.GetDefault();
            // 调用需要在UI线程运行的函数前
            WinRT.Interop.InitializeWithWindow.Initialize(context, MainWindow.Current.HWND);
            var updates = await context.GetAppAndOptionalStorePackageUpdatesAsync();
            if (updates.Any())
            {
                return (null, updates);
            }
        }
        else
        {
            // 侧载版检查更新
            var release = await _githubService.GetLatestReleaseAsync();
            if (Version.TryParse(release?.TagName, out var version))
            {
                if (version > XunkongEnvironment.AppVersion)
                {
                    return (release, null);
                }
            }
        }
        return (null, null);
    }



    /// <summary>
    /// 下载商店版更新
    /// </summary>
    /// <param name="updates"></param>
    public void RequestDownloadStoreNewVersion(IReadOnlyList<StorePackageUpdate> updates)
    {
        try
        {
            var context = StoreContext.GetDefault();
            // 调用需要在UI线程运行的函数前
            WinRT.Interop.InitializeWithWindow.Initialize(context, MainWindow.Current.HWND);
            if (updates.Any())
            {
                if (context.CanSilentlyDownloadStorePackageUpdates)
                {
                    var operation = context.TrySilentDownloadAndInstallStorePackageUpdatesAsync(updates);
                    ShowDownloadProgress(operation);
                }
                else
                {
                    var operation = context.RequestDownloadAndInstallStorePackageUpdatesAsync(updates);
                    ShowDownloadProgress(operation);
                }
            }
        }
        catch { }
    }






    /// <summary>
    /// 下载并安装新版本的进度条
    /// </summary>
    /// <param name="operation"></param>
    private static void ShowDownloadProgress(IAsyncOperationWithProgress<StorePackageUpdateResult, StorePackageUpdateStatus> operation)
    {
        const string tag = "download new version";
        const string group = "download";
        uint index = 0;
        var content = new ToastContentBuilder().AddText("别点我！").AddVisualChild(new AdaptiveProgressBar()
        {
            Title = "下载中",
            Value = new BindableProgressBarValue("progressValue"),
            ValueStringOverride = new BindableString("progressValueString"),
            Status = new BindableString("progressStatus")
        }).AddToastActivationInfo("DownloadNewVersion", ToastActivationType.Background).GetToastContent();

        var toast = new ToastNotification(content.GetXml());
        toast.Tag = tag;
        toast.Group = group;
        toast.Data = new NotificationData();
        toast.Data.Values["progressValue"] = "0";
        toast.Data.Values["progressValueString"] = "0%";
        toast.Data.Values["progressStatus"] = "0MB / 0MB";
        toast.Data.SequenceNumber = ++index;

        var manager = ToastNotificationManager.CreateToastNotifier();
        operation.Progress = (_, status) =>
        {
            if (status.PackageUpdateState == StorePackageUpdateState.Pending)
            {
                manager.Show(toast);
            }
            if (status.PackageUpdateState == StorePackageUpdateState.Downloading)
            {
                var progress = status.PackageDownloadProgress;
                var data = new NotificationData { SequenceNumber = ++index };
                data.Values["progressValue"] = $"{status.PackageDownloadProgress / 0.95}";
                data.Values["progressValueString"] = $"{status.PackageDownloadProgress / 0.95:P0}";
                data.Values["progressStatus"] = $"{status.PackageBytesDownloaded / (double)(1 << 20):F1}MB / {status.PackageDownloadSizeInBytes / (double)(1 << 20):F1}MB";
                manager.Update(data, tag, group);
            }
            if (status.PackageUpdateState == StorePackageUpdateState.Deploying)
            {
                manager.Hide(toast);
                Vanara.PInvoke.Kernel32.RegisterApplicationRestart(null, 0);
            }
        };
    }



}
