using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Xunkong.GenshinData.Achievement;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Controls;

public sealed partial class AchievementGoalFinishedPush : UserControl
{

    public AchievementGoal Goal { get; set; }

    public string? NameCardDescription { get; set; }

    public AchievementGoalFinishedPush(AchievementGoal goal)
    {
        this.InitializeComponent();
        this.Goal = goal;
        NameCardDescription = goal.RewardNameCard?.Description.Split("\\n").LastOrDefault();
    }

    private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
    {
        MainWindow.Current.CloseFullWindowContent();
    }
}
