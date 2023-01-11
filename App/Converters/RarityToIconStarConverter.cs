using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Xunkong.Desktop.Converters;

internal class RarityToIconStarConverter : IValueConverter
{

    private static readonly Uri Rarity1StarIcon = new("ms-appx:///Assets/Images/Icon_1_Stars.png");
    private static readonly Uri Rarity2StarIcon = new("ms-appx:///Assets/Images/Icon_2_Stars.png");
    private static readonly Uri Rarity3StarIcon = new("ms-appx:///Assets/Images/Icon_3_Stars.png");
    private static readonly Uri Rarity4StarIcon = new("ms-appx:///Assets/Images/Icon_4_Stars.png");
    private static readonly Uri Rarity5StarIcon = new("ms-appx:///Assets/Images/Icon_5_Stars.png");
    private static readonly Uri TransparentIcon = new("ms-appx:///Assets/Images/Transparent.png");

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var rarity = (int)value;
        var uri = rarity switch
        {
            1 => Rarity1StarIcon,
            2 => Rarity2StarIcon,
            3 => Rarity3StarIcon,
            4 => Rarity4StarIcon,
            5 => Rarity5StarIcon,
            _ => TransparentIcon,
        };
        return new BitmapImage(uri);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }


}
