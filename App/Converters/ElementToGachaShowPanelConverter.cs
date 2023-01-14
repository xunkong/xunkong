using Microsoft.UI.Xaml.Data;

namespace Xunkong.Desktop.Converters;

[Obsolete("图片不在包内", true)]
internal class ElementToGachaShowPanelConverter : IValueConverter
{
    private const string GachaShowPanel_Default = "ms-appx:///Assets/Images/UI_GachaShowPanel_Bg_Default.png";
    private const string GachaShowPanel_Fire = "ms-appx:///Assets/Images/UI_GachaShowPanel_Bg_Fire.png";
    private const string GachaShowPanel_Water = "ms-appx:///Assets/Images/UI_GachaShowPanel_Bg_Water.png";
    private const string GachaShowPanel_Wind = "ms-appx:///Assets/Images/UI_GachaShowPanel_Bg_Wind.png";
    private const string GachaShowPanel_Elect = "ms-appx:///Assets/Images/UI_GachaShowPanel_Bg_Elect.png";
    private const string GachaShowPanel_Grass = "ms-appx:///Assets/Images/UI_GachaShowPanel_Bg_Grass.png";
    private const string GachaShowPanel_Ice = "ms-appx:///Assets/Images/UI_GachaShowPanel_Bg_Ice.png";
    private const string GachaShowPanel_Rock = "ms-appx:///Assets/Images/UI_GachaShowPanel_Bg_Rock.png";

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var element = (ElementType)value;
        return element switch
        {
            ElementType.Pyro => GachaShowPanel_Fire,
            ElementType.Hydro => GachaShowPanel_Water,
            ElementType.Anemo => GachaShowPanel_Wind,
            ElementType.Electro => GachaShowPanel_Elect,
            ElementType.Dendro => GachaShowPanel_Grass,
            ElementType.Cryo => GachaShowPanel_Ice,
            ElementType.Geo => GachaShowPanel_Rock,
            _ => GachaShowPanel_Default,
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
