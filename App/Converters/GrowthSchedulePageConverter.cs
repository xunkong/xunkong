using Microsoft.UI.Xaml.Data;

namespace Xunkong.Desktop.Converters;

internal class GrowthSchedulePage_DayNumberStringToDayStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (value as string) switch
        {
            "1" => "一",
            "2" => "二",
            "3" => "三",
            "4" => "四",
            "5" => "五",
            "6" => "六",
            "7" => "日",
            _ => "？",
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}


internal class GrowthSchedulePage_IsToday : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var today = ((int)DateTimeOffset.Now.AddHours(-4).DayOfWeek);
        today = today == 0 ? 7 : today;
        return value as string == today.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}