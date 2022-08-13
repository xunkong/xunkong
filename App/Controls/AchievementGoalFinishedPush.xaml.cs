using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Controls;

public sealed partial class AchievementGoalFinishedPush : UserControl
{

    public string Title { get; set; }


    public string Image { get; set; }


    public string? NameCard { get; set; }



    public AchievementGoalFinishedPush(string title, string image, string? namecard = null)
    {
        this.InitializeComponent();
        Title = title;
        Image = image;
        NameCard = namecard;
    }

    private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
    {
        MainWindow.Current.CloseFullWindowContent();
    }
}
