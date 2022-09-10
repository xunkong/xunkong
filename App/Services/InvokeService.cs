using Microsoft.Win32;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Windows.ApplicationModel;
using Xunkong.Hoyolab;

namespace Xunkong.Desktop.Services;

internal class InvokeService
{


    /// <summary>
    /// 刷新磁贴
    /// </summary>
    /// <returns></returns>
    public static async Task RefreshDailyNoteTilesAsync()
    {
        try
        {
            var allTiles = await SecondaryTileProvider.FindAllAsync();
            var uids = allTiles.Where(x => x.Contains("DailyNote_")).Select(x => int.Parse(x.Replace("DailyNote_", ""))).ToList();
            if (uids.Any())
            {
                var service = new HoyolabService(new HoyolabClient());
                foreach (var uid in uids)
                {
                    try
                    {
                        var role = service.GetGenshinRoleInfo(uid);
                        if (role != null)
                        {
                            var info = await service.GetDailyNoteAsync(role);
                            if (info != null)
                            {
                                SecondaryTileProvider.UpdatePinnedTile(info);
                            }
                        }
                    }
                    catch (Exception ex) when (ex is HoyolabException or HttpRequestException)
                    {
                        await ToastProvider.SendAsync("刷新磁贴时遇到错误", $"Uid {uid}\n{ex.Message}");
                        Logger.Error(ex, $"刷新磁贴 - Uid {uid}");
                    }
                }
            }
            else
            {
                TaskSchedulerService.RegisterForRefreshTile(false);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "刷新磁贴");
            await ToastProvider.SendAsync("刷新磁贴时遇到错误", ex.Message);
        }
    }



    /// <summary>
    /// 启动游戏
    /// </summary>
    /// <returns></returns>
    public static async Task<bool> StartGameAsync(bool sendInAppNotification = false)
    {
        try
        {
            if (IsGameRunning())
            {
                if (sendInAppNotification)
                {
                    NotificationProvider.Warning("出错了", "已有游戏进程正在运行");
                }
                else
                {
                    await ToastProvider.SendAsync("出错了", "已有游戏进程正在运行");
                }
                return false;
            }
            var exePath = await GetGameExePathAsync();
            var fps = AppSetting.GetValue(SettingKeys.TargetFPS, 60);
            var isPopup = AppSetting.GetValue<bool>(SettingKeys.IsPopupWindow);
            if (fps > 60)
            {
                var command = new StringBuilder();
                command.Append("-exe ");
                command.Append($@"""{exePath}""");
                command.Append(" -fps ");
                command.Append(fps);
                if (isPopup)
                {
                    command.Append(" -popupwindow");
                }
                var info = new ProcessStartInfo
                {
                    FileName = Path.Combine(Package.Current.InstalledPath, @"Xunkong.Desktop.FpsUnlocker\Xunkong.Desktop.FpsUnlocker.exe"),
                    Arguments = command.ToString(),
                    UseShellExecute = true,
                    Verb = "runas",
                };
                Process.Start(info);
            }
            else
            {
                var info = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = isPopup ? "-popupwindow" : "",
                    UseShellExecute = true,
                    Verb = "runas",
                };
                Process.Start(info);
            }
            return true;
        }
        catch (Exception ex)
        {
            if (ex is Win32Exception ex32)
            {
                if (ex32.NativeErrorCode == 1223)
                {
                    // 操作已取消
                    return false;
                }
            }
            Logger.Error(ex, "启动游戏");
            if (sendInAppNotification)
            {
                NotificationProvider.Error(ex, "启动游戏");
            }
            else
            {
                await ToastProvider.SendAsync("出错了", ex.Message);
            }
            return false;
        }
    }



    public static bool IsGameRunning()
    {
        return Process.GetProcessesByName("YuanShen").Concat(Process.GetProcessesByName("GenshinImpact")).Any();
    }


    public static async Task<string> GetGameExePathAsync()
    {
        var exePath = AppSetting.GetValue<string>(SettingKeys.GameExePath);
        if (!File.Exists(exePath))
        {
            const string REG_PATH = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\原神\";
            const string REG_KEY = "InstallPath";
            var launcherPath = Registry.GetValue(REG_PATH, REG_KEY, null) as string;
            if (!string.IsNullOrWhiteSpace(launcherPath))
            {
                var configPath = Path.Combine(launcherPath, "config.ini");
                if (File.Exists(configPath))
                {
                    var str = await File.ReadAllTextAsync(configPath);
                    var gamePath = Regex.Match(str, @"game_install_path=(.+)").Groups[1].Value.Trim();
                    exePath = Path.Combine(gamePath, "YuanShen.exe");
                }
            }
            if (!File.Exists(exePath))
            {
                throw new XunkongException("没有找到 YuanShen.exe，请前往设置页面指定路径。");
            }
        }
        if (!(exePath.EndsWith("YuanShen.exe") || exePath.EndsWith("GenshinImpact.exe")))
        {
            throw new XunkongException("文件名不为 YuanShen.exe 或 GenshinImpact.exe");
        }
        return exePath;
    }



    /// <summary>
    /// 检查参量质变仪和洞天宝钱
    /// </summary>
    /// <returns></returns>
    public static async Task CheckTransformerReachedAndHomeCoinFullAsync(bool sendInAppNotification = false)
    {
        try
        {
            var service = new HoyolabService(new HoyolabClient());
            var users = service.GetGenshinRoleInfoList();
            if (!users.Any())
            {
                return;
            }
            var sb = new StringBuilder();
            foreach (var user in users)
            {
                try
                {
                    var note = await service.GetDailyNoteAsync(user);
                    if (note != null && (note.Transformer.Obtained && note.Transformer.RecoveryTime.Reached || note.IsHomeCoinFull))
                    {
                        if (note != null)
                        {
                            var text = (note.Transformer?.RecoveryTime?.Reached ?? false, note.IsHomeCoinFull) switch
                            {
                                (true, true) => $"{user.Nickname} 的洞天宝钱已满，参量质变仪可使用",
                                (true, false) => $"{user.Nickname} 的参量质变仪可使用",
                                (false, true) => $"{user.Nickname} 的洞天宝钱已满",
                                (false, false) => "",
                            };
                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                sb.AppendLine(text);
                            }
                        }
                    }
                }
                catch (HoyolabException ex)
                {
                    Logger.Error(ex, $"检查洞天宝钱和参量质变仪 - Uid {user.Uid}");
                }
            }
            var toastString = sb.ToString();
            if (!string.IsNullOrWhiteSpace(toastString))
            {
                if (sendInAppNotification)
                {
                    NotificationProvider.Warning("注意了", toastString);
                }
                else
                {
                    await ToastProvider.SendAsync("注意了", toastString);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "检查洞天宝钱和参量质变仪");
            if (sendInAppNotification)
            {
                NotificationProvider.Warning("出错了", ex.Message);
            }
            else
            {
                await ToastProvider.SendAsync("出错了", ex.Message);
            }
        }
    }


    /// <summary>
    /// 签到
    /// </summary>
    /// <returns></returns>
    public static async Task SignInAllAccountAsync()
    {
        try
        {
            var service = new HoyolabService(new HoyolabClient());
            var roles = service.GetGenshinRoleInfoList();
            foreach (var role in roles)
            {
                try
                {
                    await service.SignInAsync(role);
                }
                catch (Exception ex) when (ex is HoyolabException or HttpRequestException)
                {
                    await ToastProvider.SendAsync("签到时出现错误", $"Uid {role.Uid}\n{ex.Message}");
                    Logger.Error(ex, $"签到 - Uid {role.Uid}");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "签到时出现错误");
            await ToastProvider.SendAsync("签到时出现错误", ex.Message);
        }
    }



}
