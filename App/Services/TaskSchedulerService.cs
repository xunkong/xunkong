using Microsoft.Win32.TaskScheduler;
using Windows.Storage;

namespace Xunkong.Desktop.Services;

internal static class TaskSchedulerService
{

    /// <summary>
    /// 是否启用签到任务
    /// </summary>
    /// <param name="throwError"></param>
    /// <returns></returns>
    public static bool IsDailyCheckInEnable(bool throwError = false)
    {
        try
        {
            using var task = TaskService.Instance.GetTask("Xunkong\\每日签到");
            if (task == null)
            {
                return false;
            }
            else
            {
                return task.Enabled;
            }
        }
        catch
        {
            if (throwError)
            {
                throw;
            }
            else
            {
                return false;
            }
        }
    }


    /// <summary>
    /// 注册签到任务
    /// </summary>
    /// <param name="enable"></param>
    public static void RegisterForDailyCheckIn(bool enable)
    {
        using var task = TaskService.Instance.NewTask();
        task.RegistrationInfo.Description = "寻空每日签到任务";
        task.Actions.Add(new ExecAction("wscript", $@"/b ""{CreateWscriptFile("DailyCheckIn", "DailyCheckIn")}"""));
        task.Triggers.Add(new DailyTrigger
        {
            Enabled = true,
            DaysInterval = 1,
            StartBoundary = DateTime.Today.AddHours(6),
            RandomDelay = TimeSpan.FromMinutes(15),
        });
        task.Settings.Enabled = enable;
        task.Settings.StartWhenAvailable = true;
        using var _ = TaskService.Instance.RootFolder.RegisterTaskDefinition("Xunkong\\每日签到", task);
    }



    /// <summary>
    /// 注册刷新磁贴的任务
    /// </summary>
    /// <param name="enable"></param>
    public static void RegisterForRefreshTile(bool enable)
    {
        using var task = TaskService.Instance.NewTask();
        task.RegistrationInfo.Description = "寻空刷新磁贴任务";
        task.Actions.Add(new ExecAction("wscript", $@"/b ""{CreateWscriptFile("RefreshTile", "RefreshTile")}"""));
        task.Triggers.Add(new TimeTrigger
        {
            Enabled = true,
            Repetition = new RepetitionPattern(TimeSpan.FromMinutes(16), TimeSpan.Zero),
        });
        task.Settings.Enabled = enable;
        using var _ = TaskService.Instance.RootFolder.RegisterTaskDefinition("Xunkong\\刷新磁贴", task);
    }



    /// <summary>
    /// 创建脚本
    /// </summary>
    /// <param name="scriptName"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    private static string CreateWscriptFile(string scriptName, string args)
    {
        var script = $"""CreateObject("WScript.Shell").Run "cmd /c start shell:AppsFolder\{XunkongEnvironment.FamilyName}!App {args.Replace("\"", "\"\"")}", 0, false""";
        var folder = Path.Combine(ApplicationData.Current.TemporaryFolder.Path, "Script");
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, $"{scriptName}.vbs");
        File.WriteAllText(path, script);
        return path;
    }




}
