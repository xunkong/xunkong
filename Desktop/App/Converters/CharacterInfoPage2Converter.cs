using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace Xunkong.Desktop.Converters;

internal class RarityToBackgroundBrushConverter : IValueConverter
{

    private static SolidColorBrush Rarity5Bursh = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xF0, 0xD3));
    private static SolidColorBrush Rarity4Bursh = new SolidColorBrush(Color.FromArgb(0xFF, 0xEA, 0xD5, 0xFE));
    private static SolidColorBrush Rarity3Bursh = new SolidColorBrush(Color.FromArgb(0xFF, 0xCC, 0xD7, 0xF5));



    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            >= 5 => Rarity5Bursh,
            4 => Rarity4Bursh,
            3 => Rarity3Bursh,
            _ => null,
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}


internal class ConstellationToBackgroundBrushConverter : IValueConverter
{

    private static SolidColorBrush Constellation0Bursh = new SolidColorBrush(Color.FromArgb(0xFF, 0x66, 0x66, 0x66));
    private static SolidColorBrush Constellation1Bursh = new SolidColorBrush(Color.FromArgb(0xFF, 0x59, 0xBA, 0xC3));
    private static SolidColorBrush Constellation2Bursh = new SolidColorBrush(Color.FromArgb(0xFF, 0x33, 0x9B, 0x61));
    private static SolidColorBrush Constellation3Bursh = new SolidColorBrush(Color.FromArgb(0xFF, 0x3F, 0x94, 0xBB));
    private static SolidColorBrush Constellation4Bursh = new SolidColorBrush(Color.FromArgb(0xFF, 0x3C, 0x53, 0xBC));
    private static SolidColorBrush Constellation5Bursh = new SolidColorBrush(Color.FromArgb(0xFF, 0x73, 0x44, 0xB2));
    private static SolidColorBrush Constellation6Bursh = new SolidColorBrush(Color.FromArgb(0xFF, 0xFB, 0x56, 0x27));



    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            0 => Constellation0Bursh,
            1 => Constellation1Bursh,
            2 => Constellation2Bursh,
            3 => Constellation3Bursh,
            4 => Constellation4Bursh,
            5 => Constellation5Bursh,
            6 => Constellation6Bursh,
            _ => null,
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}


internal class WeaponAffixToBackgroundBrushConverter : IValueConverter
{

    private static SolidColorBrush Affix1Bursh = new SolidColorBrush(Color.FromArgb(0xFF, 0x33, 0x9B, 0x61));
    private static SolidColorBrush Affix2Bursh = new SolidColorBrush(Color.FromArgb(0xFF, 0x3F, 0x94, 0xBB));
    private static SolidColorBrush Affix3Bursh = new SolidColorBrush(Color.FromArgb(0xFF, 0x3C, 0x53, 0xBC));
    private static SolidColorBrush Affix4Bursh = new SolidColorBrush(Color.FromArgb(0xFF, 0x73, 0x44, 0xB2));
    private static SolidColorBrush Affix5Bursh = new SolidColorBrush(Color.FromArgb(0xFF, 0xFB, 0x56, 0x27));



    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            1 => Affix1Bursh,
            2 => Affix2Bursh,
            3 => Affix3Bursh,
            4 => Affix4Bursh,
            5 => Affix5Bursh,
            _ => null,
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}



internal class FetterToBackgroundColorConverter : IValueConverter
{


    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            10 => Colors.Red,
            _ => Colors.Purple,
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}



internal class FetterToBackgroundBrushConverter : IValueConverter
{

    private static SolidColorBrush RedBursh = new SolidColorBrush(Colors.Red);
    private static SolidColorBrush PurpleBursh = new SolidColorBrush(Colors.Purple);

    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            10 => RedBursh,
            _ => PurpleBursh,
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}


internal class SkillLevelToBackgroundBrushConverter : IValueConverter
{

    private static SolidColorBrush Level1234Bursh = new SolidColorBrush(Color.FromArgb(0xFF, 0xC4, 0xC4, 0xC4));
    private static SolidColorBrush Level56Bursh = new SolidColorBrush(Color.FromArgb(0xFF, 0x83, 0xD3, 0x94));
    private static SolidColorBrush Level78Bursh = new SolidColorBrush(Color.FromArgb(0xFF, 0x8C, 0xBF, 0xEA));
    private static SolidColorBrush Level9Bursh = new SolidColorBrush(Color.FromArgb(0xFF, 0xC3, 0xA5, 0xFF));
    private static SolidColorBrush Level10Bursh = new SolidColorBrush(Color.FromArgb(0xFF, 0xF9, 0x7E, 0x7E));

    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            1 or 2 or 3 or 4 => Level1234Bursh,
            5 or 6 => Level56Bursh,
            7 or 8 => Level78Bursh,
            9 => Level9Bursh,
            10 => Level10Bursh,
            _ => null,
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}