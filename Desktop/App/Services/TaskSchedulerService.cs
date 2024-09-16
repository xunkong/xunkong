using Microsoft.Win32.TaskScheduler;
using Windows.ApplicationModel.Background;
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
    public static void RegisterForRefreshTile()
    {
        if (AppSetting.GetValue(SettingKeys.EnableDailyNoteTask, true))
        {
            foreach (var item in BackgroundTaskRegistration.AllTasks)
            {
                if (item.Value.Name == "DailyNoteTask")
                {
                    return;
                }
            }
            int interval = AppSetting.GetValue(SettingKeys.DailyNoteTaskTimeInterval, 16);
            interval = Math.Clamp(interval, 15, int.MaxValue);
            var builder = new BackgroundTaskBuilder();
            builder.Name = "DailyNoteTask";
            builder.TaskEntryPoint = "Xunkong.Desktop.BackgroundTask.DailyNoteTask";
            builder.SetTrigger(new Windows.ApplicationModel.Background.TimeTrigger((uint)interval, false));
            _ = builder.Register();
        }
    }


    /// <summary>
    /// 取消注册刷新磁贴的任务
    /// </summary>
    public static void UnegisterForRefreshTile()
    {
        foreach (var item in BackgroundTaskRegistration.AllTasks)
        {
            if (item.Value.Name == "DailyNoteTask")
            {
                item.Value.Unregister(false);
            }
        }
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
