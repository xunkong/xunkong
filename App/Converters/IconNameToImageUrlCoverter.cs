using Microsoft.UI.Xaml.Data;

namespace Xunkong.Desktop.Converters;

internal class IconNameToImageUrlCoverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var name = value as string;
        if (name != null)
        {
            if (name.StartsWith("https://"))
            {
                return name;
            }
            else
            {
                if (name.Contains("Avatar"))
                {
                    if (name == "UI_AvatarIcon_Momoka")
                    {
                        return "https://act-webstatic.mihoyo.com/hk4e/e20200928calculate/item_icon_u587xe/4d7a0b2fd4fe00ce27d07d10a78c0e63.png";
                    }
                    return $"https://upload-bbs.mihoyo.com/game_record/genshin/character_icon/{name}.png";

                }
                if (name.Contains("Equip"))
                {
                    return $"https://upload-bbs.mihoyo.com/game_record/genshin/equip/{name}.png";
                }
            }
        }
        return null!;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
