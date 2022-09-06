using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Xunkong.Hoyolab;
using Xunkong.Hoyolab.Activity;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class AnnouncementPage : Page
{

    private readonly HoyolabClient _hoyolabClient;
    public AnnouncementPage()
    {
        this.InitializeComponent();
        _hoyolabClient = ServiceProvider.GetService<HoyolabClient>()!;
        Loaded += AnnouncementPage_Loaded;
    }


    [ObservableProperty]
    private List<Announcement> activityAnnouncements;


    [ObservableProperty]
    private List<Announcement> gameAnnouncements;


    private async void AnnouncementPage_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var announces = await _hoyolabClient.GetGameAnnouncementsAsync();
            ActivityAnnouncements = announces.Where(x => x.Type == 1).OrderBy(x => x.EndTime).ToList();
            GameAnnouncements = announces.Where(x => x.Type == 2).ToList();
        }
        catch (Exception ex)
        {

        }
    }

    private void c_Grid_FinishingActivity_Tapped(object sender, TappedRoutedEventArgs e)
    {
        try
        {
            if (sender is Grid grid)
            {
                if (grid.DataContext is Announcement announce)
                {
                    c_AnnouncementContentViewer.Announce = announce;
                    FlyoutBase.ShowAttachedFlyout(c_Grid_Base);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "显示活动内容");
        }
    }
}
