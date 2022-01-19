using System.Security.Cryptography;
using System.Text;
using Windows.ApplicationModel;
using Windows.Storage;

namespace Xunkong.Desktop.Models
{
    internal static class XunkongEnvironment
    {

        public static readonly string AppName;

        public static readonly Version AppVersion;

        public static readonly string DeviceId;

        public static readonly string? UserDataPath;

        public static readonly string? WebView2Version;

        static XunkongEnvironment()
        {
            AppName = Package.Current.DisplayName;
            var v = Package.Current.Id.Version;
            AppVersion = new Version(v.Major, v.Minor, v.Build, v.Revision);

            var UserName = Environment.UserName;
            var MachineGuid = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography\", "MachineGuid", UserName);
            var bytes = Encoding.UTF8.GetBytes(UserName + MachineGuid);
            var hash = MD5.HashData(bytes);
            DeviceId = Convert.ToHexString(hash);

            UserDataPath = ApplicationData.Current.LocalSettings.Values[SettingKeys.UserDataPath] as string;
            try
            {
                WebView2Version = Microsoft.Web.WebView2.Core.CoreWebView2Environment.GetAvailableBrowserVersionString();
            }
            catch { }
        }


        public static string GetLogHeader()
        {
            var sb = new StringBuilder();
            sb.AppendLine(DateTimeOffset.Now.ToString("O"));
            sb.AppendLine("Xunkong Desktop Log Message");
            sb.AppendLine($"App Name: {AppName}");
            sb.AppendLine($"App Version: {AppVersion}");
            sb.AppendLine($"OS Version: {Environment.OSVersion}");
            sb.AppendLine($"Device Id: {DeviceId}");
            sb.AppendLine($"Data Path: {UserDataPath}");
            sb.AppendLine($"WebView2 Version: {WebView2Version ?? "Null"}");
            return sb.ToString();
        }
    }
}
