using System.Diagnostics;
using Xunkong.Hoyolab;

namespace Xunkong.Desktop.Helpers;

public static class DeviceFpExtension
{

    public static async Task UpdateDeviceFpAsync(this HoyolabClient _hoyolabClient, bool forceUpdate = false)
    {
        try
        {
            string? id = AppSetting.GetValue<string>(SettingKeys.HoyolabDeviceId);
            string? fp = AppSetting.GetValue<string>(SettingKeys.HoyolabDeviceFp);
            DateTimeOffset lastUpdateTime = AppSetting.GetValue<DateTimeOffset>(SettingKeys.HoyolabDeviceFpLastUpdateTime);
            if (!forceUpdate && !string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(fp))
            {
                _hoyolabClient.DeviceId = id;
                _hoyolabClient.DeviceFp = fp;
            }
            if (forceUpdate || DateTimeOffset.Now - lastUpdateTime > TimeSpan.FromDays(3))
            {
                await _hoyolabClient.GetDeviceFpAsync();
                AppSetting.SetValue(SettingKeys.HoyolabDeviceId, _hoyolabClient.DeviceId);
                AppSetting.SetValue(SettingKeys.HoyolabDeviceFp, _hoyolabClient.DeviceFp);
                AppSetting.SetValue(SettingKeys.HoyolabDeviceFpLastUpdateTime, DateTimeOffset.Now);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }




}
