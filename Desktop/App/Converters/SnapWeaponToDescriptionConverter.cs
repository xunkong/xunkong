using Microsoft.UI.Xaml.Data;

namespace Xunkong.Desktop.Converters;

internal class SnapWeaponToDescriptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            1 => "单手剑",
            10 => "法器",
            11 => "双手剑",
            12 => "弓箭",
            13 => "长柄武器",
            _ => "",
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
