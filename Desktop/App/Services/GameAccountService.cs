using Microsoft.Win32;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

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
        OperationHistory.AddToDatabase("ChangeGameAccount", account.Server.ToString(), account.Name);
        Logger.TrackEvent("ChangeGameAccount", "Server", account.Server.ToString());
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
    /// <param name="server">0-cn, 1-global, 2-cncloud</param>
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
                0 => @"HKEY_CURRENT_USER\Software\miHoYo\HYP\1_1\hk4e_cn",
                1 => @"HKEY_CURRENT_USER\Software\Cognosphere\HYP\1_0\hk4e_global",
                2 => @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\云·原神",
                _ => throw new ArgumentOutOfRangeException(nameof(server)),
            };
            exePath = GetGameExePathFromRegistry(server, regKey);
        }
        if (!File.Exists(exePath))
        {
            throw new XunkongException($"没有找到{((GameAccount.GameServer)server).ToDescription()}的游戏文件，请在设置页面指定路径。");
        }
        return exePath;
    }




    /// <summary>
    /// 从注册表查找游戏路径
    /// </summary>
    /// <param name="key"></param>
    /// <param name="isCloud">云原神</param>
    /// <returns></returns>
    private static string? GetGameExePathFromRegistry(int server, string key)
    {
        var launcherPath = Registry.GetValue(key, "InstallPath", null) as string;
        if (server == 0)
        {
            string? folder = Registry.GetValue(key, "GameInstallPath", null) as string;
            string? exe = Path.Join(folder, "YuanShen.exe");
            if (File.Exists(exe))
            {
                return exe;
            }
        }
        else if (server == 1)
        {
            string? folder = Registry.GetValue(key, "GameInstallPath", null) as string;
            string? exe = Path.Join(folder, "GenshinImpact.exe");
            if (File.Exists(exe))
            {
                return exe;
            }
        }
        else if (server == 2)
        {
            var exeName = Registry.GetValue(key, "ExeName", null) as string;
            var exePath = Path.Join(launcherPath, exeName);
            if (File.Exists(exePath))
            {
                return exePath;
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
                //var fps = AppSetting.GetValue(SettingKeys.TargetFPS, 60);
                var isPopup = AppSetting.GetValue<bool>(SettingKeys.IsPopupWindow);
                var width = AppSetting.GetValue<int>(SettingKeys.StartGameWindowWidth);
                var height = AppSetting.GetValue<int>(SettingKeys.StartGameWindowHeight);
                var command = new StringBuilder();
                if (isPopup)
                {
                    command.Append("-popupwindow ");
                }
                if (width > 0 && height > 0)
                {
                    command.Append($"-screen-width {width} -screen-height {height} ");
                }
                var info = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = command.ToString(),
                    UseShellExecute = true,
                    Verb = "runas",
                    WorkingDirectory = Path.GetDirectoryName(exePath),
                };
                Process.Start(info);
            }
            OperationHistory.AddToDatabase("StartGame", ((GameAccount.GameServer)server).ToString());
            Logger.TrackEvent("StartGame", "Server", ((GameAccount.GameServer)server).ToString());
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




    public static void ChangeGameAccountAndStartGameAsync(string sha256)
    {
        try
        {
            using var dapper = DatabaseProvider.CreateConnection();
            var account = dapper.QueryFirstOrDefault<GameAccount>("SELECT * FROM GameAccount WHERE SHA256=@sha256 LIMIT 1;", new { sha256 });
            if (account is null)
            {
                ToastProvider.Send("没有找到相关账号的信息");
                return;
            }
            if (IsGameRunning((int)account.Server))
            {
                ToastProvider.Send("已有游戏进程正在运行");
                return;
            }
            if (!ChangeGameAccount(account))
            {
                ToastProvider.Send("切换账号失败");
            }
            StartGameAsync((int)account.Server).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            ToastProvider.Send(ex.Message);
        }
    }



}
