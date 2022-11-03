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

    private readonly SystemBackdropProperty micaAltBackdropProperty;

    private readonly SystemBackdropProperty acrylicBackdropProperty;



    public SystemBackdropHelper(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        this.window = window;
        backdrop = new(window);
        micaAltBackdropProperty = SystemBackdropProperty.MicaAltDefault with { TintColorDark = 0xFF141414, TintOpacityLight = 0 };
        acrylicBackdropProperty = SystemBackdropProperty.AcrylicDefault with { TintColorLight = 0xFFFCFCFC, TintColorDark = 0xFF2C2C2C };
    }



    /// <summary>
    /// 启动时设置背景材质
    /// </summary>
    /// <returns>设置成功</returns>
    public bool TrySetBackdrop()
    {
        if (AppSetting.TryGetValue(SettingKeys.WindowBackdrop, out uint value))
        {
            var alwaysActive = (value & 0x80000000) > 0;
            switch (value & 0xF)
            {
                case 1:
                    backdrop.SetBackdropProperty();
                    return backdrop.TrySetMica(alwaysActive: alwaysActive);
                case 2:
                    backdrop.SetBackdropProperty(acrylicBackdropProperty);
                    return backdrop.TrySetAcrylic(alwaysActive: alwaysActive);
                case 3:
                    backdrop.SetBackdropProperty(micaAltBackdropProperty);
                    return backdrop.TrySetMica(useMicaAlt: true, alwaysActive: alwaysActive);
                default:
                    return false;
            }
        }
        else
        {
            return false;
        }
    }



    /// <summary>
    /// 修改背景材质
    /// </summary>
    /// <param name="value">设置值，最高位：一直激活，低位：材质值</param>
    /// <param name="backdropType">背景材质类型</param>
    /// <returns></returns>
    public bool TryChangeBackdrop(uint value, out uint backdropType)
    {
        var alwaysActive = (value & 0x80000000) > 0;
        backdropType = value & 0xF;
        switch (backdropType)
        {
            case 1:
                backdrop.SetBackdropProperty();
                return backdrop.TrySetMica(alwaysActive: alwaysActive);
            case 2:
                backdrop.SetBackdropProperty(acrylicBackdropProperty);
                return backdrop.TrySetAcrylic(alwaysActive: alwaysActive);
            case 3:
                backdrop.SetBackdropProperty(micaAltBackdropProperty);
                return backdrop.TrySetMica(useMicaAlt: true, alwaysActive: alwaysActive);
            default:
                backdrop.ResetBackdrop();
                return true;
        }
    }





}