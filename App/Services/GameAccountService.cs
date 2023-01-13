using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace Xunkong.Desktop.Services;

internal class GameAccountService
{

    private const string CnPath = "原神";
    private const string CnADL = "MIHOYOSDK_ADL_PROD_CN_h3123967166";

    private const string CnCloudPath = "云·原神";
    private const string CnCloudADL = "MIHOYOSDK_ADL_0";

    private const string GlobalPath = "Genshin Impact";
    private const string GlobalADL = "MIHOYOSDK_ADL_PROD_OVERSEA_h1158948810";

    private const string QueryNameByHash = "SELECT Name FROM GameAccount WHERE SHA256=@SHA256 LIMIT 1;";


    public static List<GameAccount> GetGameAccountsFromRegistry()
    {
        var list = new List<GameAccount>();
        using var dapper = DatabaseProvider.CreateConnection();

        var cnKey = Registry.GetValue(@$"HKEY_CURRENT_USER\Software\miHoYo\{CnPath}", CnADL, null) as byte[];
        if (cnKey != null)
        {
            var account = new GameAccount
            {
                SHA256 = Convert.ToHexString(SHA256.HashData(cnKey)),
                Server = GameAccount.GameServer.CN,
                Value = cnKey,
            };
            account.Name = dapper.QueryFirstOrDefault<string>(QueryNameByHash, new { account.SHA256 });
            list.Add(account);
        }
        var cnCloudKey = Registry.GetValue(@$"HKEY_CURRENT_USER\Software\miHoYo\{CnCloudPath}", CnCloudADL, null) as byte[];
        if (cnCloudKey != null)
        {
            var account = new GameAccount
            {
                SHA256 = Convert.ToHexString(SHA256.HashData(cnCloudKey)),
                Server = GameAccount.GameServer.CNCloud,
                Value = cnCloudKey,
            };
            account.Name = dapper.QueryFirstOrDefault<string>(QueryNameByHash, new { account.SHA256 });
            list.Add(account);
        }

        var globalKey = Registry.GetValue(@$"HKEY_CURRENT_USER\Software\miHoYo\{GlobalPath}", GlobalADL, null) as byte[];
        if (globalKey != null)
        {
            var account = new GameAccount
            {
                SHA256 = Convert.ToHexString(SHA256.HashData(globalKey)),
                Server = GameAccount.GameServer.Global,
                Value = globalKey,
            };
            account.Name = dapper.QueryFirstOrDefault<string>(QueryNameByHash, new { account.SHA256 });
            list.Add(account);
        }

        return list;
    }



    public static IEnumerable<GameAccount> GetGameAccountsFromDatabase()
    {
        using var dapper = DatabaseProvider.CreateConnection();
        return dapper.Query<GameAccount>("SELECT * FROM GameAccount ORDER BY Server;");
    }



    public static void SaveGameAccount(GameAccount account)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        dapper.Execute("INSERT OR REPLACE INTO GameAccount (SHA256, Name, Server, Value, Time) VALUES (@SHA256, @Name, @Server, @Value, @Time);", account);
    }



    public static void DeleteGameAccount(GameAccount account)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        dapper.Execute("DELETE FROM GameAccount WHERE SHA256=@SHA256;", account);
    }



    public static bool ChangeGameAccount(GameAccount account)
    {
        if (IsGameRunning((int)account.Server))
        {
            throw new XunkongException("游戏正在运行，无法切换账号");
        }
        var base64 = Convert.ToBase64String(account.Value);
        var key = (int)account.Server switch
        {
            0 => CnPath,
            1 => GlobalPath,
            2 => CnCloudPath,
            _ => throw new ArgumentOutOfRangeException(nameof(account.Server)),
        };
        var name = (int)account.Server switch
        {
            0 => CnADL,
            1 => GlobalADL,
            2 => CnCloudADL,
            _ => throw new ArgumentOutOfRangeException(nameof(account.Server)),
        };
        var script = $"""
            $value = [Convert]::FromBase64String('{base64}');
            Set-ItemProperty -Path 'HKCU:\Software\miHoYo\{key}' -Name '{name}' -Value $value -Force;
            """;
        Process.Start(new ProcessStartInfo
        {
            FileName = "PowerShell",
            Arguments = script,
            CreateNoWindow = true,
        })?.WaitForExit();
        var value = Registry.GetValue($@"HKEY_CURRENT_USER\Software\miHoYo\{key}", name, null) as byte[];
        if (value != null && value.SequenceEqual(account.Value))
        {
            return true;
        }
        else
        {
            return false;
        }
    }





    /// <summary>
    /// 游戏是否在运行
    /// </summary>
    /// <param name="server"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static bool IsGameRunning(int server)
    {
        return (server switch
        {
            0 => Process.GetProcessesByName("YuanShen"),
            1 => Process.GetProcessesByName("GenshinImpact"),
            2 => Process.GetProcessesByName("Genshin Impact Cloud Game"),
            _ => throw new ArgumentOutOfRangeException(nameof(server)),
        }).Any();
    }



    /// <summary>
    /// 游戏路径
    /// </summary>
    /// <param name="server"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="XunkongException"></exception>
    public static string GetGameExePath(int server)
    {
        var settingKey = server switch
        {
            0 => SettingKeys.GameExePathCN,
            1 => SettingKeys.GameExePathGlobal,
            2 => SettingKeys.GameExePathCNCloud,
            _ => throw new ArgumentOutOfRangeException(nameof(server)),
        };
        var exePath = AppSetting.GetValue<string>(settingKey);
        if (!File.Exists(exePath))
        {
            var regKey = server switch
            {
                0 => @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\原神",
                1 => @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Genshin Impact",
                2 => @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\云·原神",
                _ => throw new ArgumentOutOfRangeException(nameof(server)),
            };
            exePath = GetGameExePathFromRegistry(regKey, server == 2);
        }
        if (!File.Exists(exePath))
        {
            throw new XunkongException($"没有找到{(GameAccount.GameServer)server}的游戏文件，请在设置页面指定路径。");
        }
        return exePath;
    }




    /// <summary>
    /// 从注册表查找游戏路径
    /// </summary>
    /// <param name="key"></param>
    /// <param name="isCloud">云原神</param>
    /// <returns></returns>
    private static string? GetGameExePathFromRegistry(string key, bool isCloud)
    {
        var launcherPath = Registry.GetValue(key, "InstallPath", null) as string;
        if (isCloud)
        {
            var exeName = Registry.GetValue(key, "ExeName", null) as string;
            var exePath = Path.Join(launcherPath, exeName);
            if (File.Exists(exePath))
            {
                return exePath;
            }
        }
        else
        {
            var configPath = Path.Join(launcherPath, "config.ini");
            if (File.Exists(configPath))
            {
                var str = File.ReadAllText(configPath);
                var installPath = Regex.Match(str, @"game_install_path=(.+)").Groups[1].Value.Trim();
                var exeName = Regex.Match(str, @"game_start_name=(.+)").Groups[1].Value.Trim();
                var exePath = Path.Join(installPath, exeName);
                if (File.Exists(exePath))
                {
                    return exePath;
                }
            }
        }
        return null;
    }




    /// <summary>
    /// 启动游戏
    /// </summary>
    /// <returns></returns>
    public static async Task<bool> StartGameAsync(int server, bool sendInAppNotification = false)
    {
        try
        {
            if (IsGameRunning(server))
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
            var exePath = GetGameExePath(server);
            if (server == 2)
            {
                Process.Start(exePath);
            }
            else
            {
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
                        FileName = Path.Combine(Package.Current.InstalledLocation.Path, @"Xunkong.Desktop.FpsUnlocker\Xunkong.Desktop.FpsUnlocker.exe"),
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
            }
            OperationHistory.AddToDatabase("StartGame", ((GameAccount.GameServer)server).ToString());
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





}
