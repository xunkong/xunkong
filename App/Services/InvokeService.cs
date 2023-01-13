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
                    catch (HoyolabException ex)
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
