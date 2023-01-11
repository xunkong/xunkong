using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Xunkong.Desktop.Converters;

internal class RarityToBackgroundImageConverter : IValueConverter
{

    private static readonly Uri Rarity1Background = new("ms-appx:///Assets/Images/Rarity_1_Background.png");
    private static readonly Uri Rarity2Background = new("ms-appx:///Assets/Images/Rarity_2_Background.png");
    private static readonly Uri Rarity3Background = new("ms-appx:///Assets/Images/Rarity_3_Background.png");
    private static readonly Uri Rarity4Background = new("ms-appx:///Assets/Images/Rarity_4_Background.png");
    private static readonly Uri Rarity5Background = new("ms-appx:///Assets/Images/Rarity_5_Background.png");
    private static readonly Uri TransparentBackground = new("ms-appx:///Assets/Images/Transparent.png");

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var rarity = (int)value;
        var uri = rarity switch
        {
            1 => Rarity1Background,
            2 => Rarity2Background,
            3 => Rarity3Background,
            4 => Rarity4Background,
            5 => Rarity5Background,
            _ => TransparentBackground,
        };
        return new BitmapImage(uri);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }


}
