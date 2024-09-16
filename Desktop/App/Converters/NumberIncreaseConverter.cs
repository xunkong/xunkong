using Microsoft.UI.Xaml.Data;

namespace Xunkong.Desktop.Converters;

internal class NumberIncreaseConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int INT)
        {
            return $"{++INT}";
        }
        if (value is uint UINT)
        {
            return $"{++UINT}UINT";
        }
        if (value is double DOUBLE)
        {
            return $"{++DOUBLE}";
        }
        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
