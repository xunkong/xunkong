using Microsoft.UI.Xaml.Data;

namespace Xunkong.Desktop.Converters;

internal class BirthDayToBirthDayStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string { Length: > 0 } str)
        {
            return $"{str.Split("/").FirstOrDefault()}月{str.Split("/").LastOrDefault()}日";
        }
        else
        {
            return "——";
        }

    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
