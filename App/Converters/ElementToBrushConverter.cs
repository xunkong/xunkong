using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace Xunkong.Desktop.Converters;

internal class ElementToBrushConverter : IValueConverter
{

    private static SolidColorBrush Element_Fire = new SolidColorBrush(Color.FromArgb(0xFF, 0xEE, 0x69, 0x46));
    private static SolidColorBrush Element_Water = new SolidColorBrush(Color.FromArgb(0xFF, 0x47, 0x8D, 0xCD));
    private static SolidColorBrush Element_Wind = new SolidColorBrush(Color.FromArgb(0xFF, 0x59, 0xA4, 0xA7));
    private static SolidColorBrush Element_Electro = new SolidColorBrush(Color.FromArgb(0xFF, 0x85, 0x75, 0xCB));
    private static SolidColorBrush Element_Grass = new SolidColorBrush(Color.FromArgb(0xFF, 0x7F, 0xB3, 0x45));
    private static SolidColorBrush Element_Ice = new SolidColorBrush(Color.FromArgb(0xFF, 0x47, 0xC1, 0xD9));
    private static SolidColorBrush Element_Rock = new SolidColorBrush(Color.FromArgb(0xFF, 0xCF, 0x9A, 0x58));

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var element = (ElementType)value;
        return element switch
        {
            ElementType.Pyro => Element_Fire,
            ElementType.Hydro => Element_Water,
            ElementType.Anemo => Element_Wind,
            ElementType.Electro => Element_Electro,
            ElementType.Dendro => Element_Grass,
            ElementType.Cryo => Element_Ice,
            ElementType.Geo => Element_Rock,
            _ => Application.Current.Resources["TextFillColorSecondaryBrush"],
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }


}
