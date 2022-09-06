using Microsoft.UI.Xaml.Controls;
using Xunkong.Hoyolab.Account;
using Xunkong.Hoyolab.DailyNote;
using Xunkong.Hoyolab.TravelNotes;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Controls;

[INotifyPropertyChanged]
public sealed partial class DailyNoteThumbCard : UserControl
{


    public DailyNoteThumbCard()
    {
        this.InitializeComponent();
    }



    public HoyolabUserInfo HoyolabUserInfo { get; set; }


    public GenshinRoleInfo GenshinRoleInfo { get; set; }


    public DailyNoteInfo DailyNoteInfo { get; set; }


    public TravelNotesDayData TravelNotesDayData { get; set; }


    public bool Error { get; set; }


    public string ErrorMessage { get; set; }



}
