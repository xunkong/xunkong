using Microsoft.UI.Xaml.Data;

namespace Xunkong.Desktop.Converters;

internal class SnapImageUrlConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string url)
        {
            if (url.Contains("/") || url.Contains(@"\"))
            {
                return url;
            }
            return $"https://file.xunkong.cc/assets/img/{url}.png";
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
