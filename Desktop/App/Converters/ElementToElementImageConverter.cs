using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Xunkong.Desktop.Converters;

internal class ElementToElementImageConverter : IValueConverter
{

    private static readonly Uri Element_Pyro = new("ms-appx:///Assets/Images/Element_Pyro.png");
    private static readonly Uri Element_Hydro = new("ms-appx:///Assets/Images/Element_Hydro.png");
    private static readonly Uri Element_Anemo = new("ms-appx:///Assets/Images/Element_Anemo.png");
    private static readonly Uri Element_Electro = new("ms-appx:///Assets/Images/Element_Electro.png");
    private static readonly Uri Element_Dendro = new("ms-appx:///Assets/Images/Element_Dendro.png");
    private static readonly Uri Element_Cryo = new("ms-appx:///Assets/Images/Element_Cryo.png");
    private static readonly Uri Element_Geo = new("ms-appx:///Assets/Images/Element_Geo.png");
    private static readonly Uri Element_None = new("ms-appx:///Assets/Images/Transparent.png");

    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        var element = (ElementType)value;
        var uri = element switch
        {
            ElementType.Pyro => Element_Pyro,
            ElementType.Hydro => Element_Hydro,
            ElementType.Anemo => Element_Anemo,
            ElementType.Electro => Element_Electro,
            ElementType.Dendro => Element_Dendro,
            ElementType.Cryo => Element_Cryo,
            ElementType.Geo => Element_Geo,
            _ => Element_None,
        };
        return new BitmapImage(uri);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}



internal class SnapElementToElementImageConverter : IValueConverter
{

    private static readonly Uri Element_Pyro = new("ms-appx:///Assets/Images/Element_Pyro.png");
    private static readonly Uri Element_Hydro = new("ms-appx:///Assets/Images/Element_Hydro.png");
    private static readonly Uri Element_Anemo = new("ms-appx:///Assets/Images/Element_Anemo.png");
    private static readonly Uri Element_Electro = new("ms-appx:///Assets/Images/Element_Electro.png");
    private static readonly Uri Element_Dendro = new("ms-appx:///Assets/Images/Element_Dendro.png");
    private static readonly Uri Element_Cryo = new("ms-appx:///Assets/Images/Element_Cryo.png");
    private static readonly Uri Element_Geo = new("ms-appx:///Assets/Images/Element_Geo.png");
    private static readonly Uri Element_None = new("ms-appx:///Assets/Images/Transparent.png");

    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        var uri = value switch
        {
            "火" => Element_Pyro,
            "水" => Element_Hydro,
            "风" => Element_Anemo,
            "雷" => Element_Electro,
            "草" => Element_Dendro,
            "冰" => Element_Cryo,
            "岩" => Element_Geo,
            _ => Element_None,
        };
        return new BitmapImage(uri);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}