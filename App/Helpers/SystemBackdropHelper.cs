using Microsoft.UI.Xaml;
using Scighost.WinUILib.Helpers;

namespace Xunkong.Desktop.Helpers;


/// <summary>
/// 系统背景帮助类
/// </summary>
public class SystemBackdropHelper
{
    private readonly Window window;

    private readonly SystemBackdrop backdrop;


    public SystemBackdropHelper(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        this.window = window;
        backdrop = new(window);
    }



    /// <summary>
    /// 应用启动时使用
    /// </summary>
    /// <returns></returns>
    public bool TrySetBackdrop()
    {
        if (AppSetting.TryGetValue(SettingKeys.WindowBackdrop, out uint value))
        {
            var alwaysActive = (value & 0x80000000) > 0;
            return (value & 0xF) switch
            {
                1 => backdrop.TrySetMica(alwaysActive: alwaysActive),
                2 => backdrop.TrySetAcrylic(alwaysActive: alwaysActive),
                3 => backdrop.TrySetMica(useMicaAlt: true, alwaysActive: alwaysActive),
                _ => false,
            };
        }
        else
        {
            return false;
        }
    }



    /// <summary>
    /// 修改设置时使用
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryChangeBackdrop(uint value)
    {
        var alwaysActive = (value & 0x80000000) > 0;
        return (value & 0xF) switch
        {
            1 => backdrop.TrySetMica(alwaysActive: alwaysActive),
            2 => backdrop.TrySetAcrylic(alwaysActive: alwaysActive),
            3 => backdrop.TrySetMica(useMicaAlt: true, alwaysActive: alwaysActive),
            _ => backdrop.Reset(),
        };
    }





}