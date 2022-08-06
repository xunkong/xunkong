using Serilog;
using Serilog.Context;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Xunkong.Desktop.Helpers;

internal static class Logger
{

    private static bool _initialized;


    private static void Initialize()
    {
        try
        {
            var path = Path.Combine(XunkongEnvironment.UserDataPath, $@"Log\App\log_{DateTimeOffset.Now:yyyyMMdd_HHmmss}.txt");
            Log.Logger = new LoggerConfiguration().WriteTo.File(path: path, outputTemplate: "{NewLine}[{Timestamp:HH:mm:ss.fff}] [{Level:u4}] {CallerName}{NewLine}{Message}{NewLine}{Exception}")
                                                  .Enrich.FromLogContext()
                                                  .CreateLogger();
            Log.Information($"{XunkongEnvironment.AppName} {XunkongEnvironment.AppVersion}\n{XunkongEnvironment.DeviceId}");
            _initialized = true;
        }
        catch { }
    }



    public static void Error(Exception ex, string? message = null, [CallerMemberName] string callerMemberName = "")
    {
        if (!_initialized)
        {
            Initialize();
        }
        try
        {
            var stack = new StackFrame(1);
            var callerName = $"{stack.GetMethod()?.DeclaringType?.FullName}.{callerMemberName}";
            using (LogContext.PushProperty("CallerName", callerName))
            {
                Log.Error(ex, message);
            }
        }
        catch { }
    }

}
