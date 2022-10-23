using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Xunkong.Desktop.Converters;

internal class PlayerToIconConverter : IValueConverter
{


    private static BitmapImage PlayerBoy = new BitmapImage(new Uri("ms-appx:///Assets/Images/SpriteUI_BtnIcon_PlayerBoy.png"));
    private static BitmapImage PlayerGirl = new BitmapImage(new Uri("ms-appx:///Assets/Images/SpriteUI_BtnIcon_PlayerGirl.png"));


    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            1 => PlayerBoy,
            2 => PlayerGirl,
            _ => null!,
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
