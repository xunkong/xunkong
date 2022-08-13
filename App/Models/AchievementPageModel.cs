using Xunkong.GenshinData.Achievement;

namespace Xunkong.Desktop.Models;

[INotifyPropertyChanged]
public partial class AchievementPageModel_Goal : AchievementGoal
{

    public List<AchievementPageModel_Item> Items { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFinish))]
    [NotifyPropertyChangedFor(nameof(Progress))]
    private int current;

    [ObservableProperty]
    private int total;

    [ObservableProperty]
    private DateTimeOffset finishedTime;

    public bool IsFinish => Current >= Total;

    public string Progress => $"{(int)((double)Current / Total * 100)}%";

    public string FinishedTimeString => FinishedTime.ToString("yyyy/MM/dd");

}



[INotifyPropertyChanged]
public partial class AchievementPageModel_Item : AchievementItem
{

    public int NextAchievementId { get; set; }

    public int MaxStar { get; set; }

    public int CurrentStar { get; set; }

    public int Uid { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Icon))]
    [NotifyPropertyChangedFor(nameof(IsFinish))]
    private int current;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Icon))]
    [NotifyPropertyChangedFor(nameof(IsFinish))]
    private int status;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FinishedDateString))]
    [NotifyPropertyChangedFor(nameof(FinishedTimeString))]
    public DateTimeOffset finishedTime;

    [ObservableProperty]
    public string comment;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LastUpdateTimeString))]
    public DateTimeOffset lastUpdateTime;

    [ObservableProperty]
    private bool notShow;

    [ObservableProperty]
    private bool editable;

    public bool IsFinish => (Status > 1) || (Current >= Progress);

    public string FinishedDateString => FinishedTime.ToLocalTime().ToString("yyyy/MM/dd");

    private static DateTimeOffset StartTime = new DateTimeOffset(2020, 9, 1, 0, 0, 0, TimeSpan.Zero);

    public string? FinishedTimeString => FinishedTime > StartTime ? FinishedTime.ToLocalTime().ToString("yyyy/MM/dd HH:mm:ss") : null;

    public string LastUpdateTimeString => LastUpdateTime > StartTime ? LastUpdateTime.ToLocalTime().ToString("yyyy/MM/dd HH:mm:ss") : "";

    public bool ShowNumberBox => !IsDeleteWatcherAfterFinish;


    public string Icon => (MaxStar, CurrentStar, IsFinish) switch
    {
        (1, _, false) => "ms-appx:///Assets/Images/UI_AchievementIcon_1_0.png",
        (1, _, true) => "ms-appx:///Assets/Images/UI_AchievementIcon_1_1.png",
        (2, _, true) => "ms-appx:///Assets/Images/UI_AchievementIcon_2_2.png",
        (3, _, true) => "ms-appx:///Assets/Images/UI_AchievementIcon_3_3.png",
        (2, 0, _) => "ms-appx:///Assets/Images/UI_AchievementIcon_2_0.png",
        (2, 1, _) => "ms-appx:///Assets/Images/UI_AchievementIcon_2_1.png",
        (2, 2, _) => "ms-appx:///Assets/Images/UI_AchievementIcon_2_2.png",
        (3, 0, _) => "ms-appx:///Assets/Images/UI_AchievementIcon_3_0.png",
        (3, 1, _) => "ms-appx:///Assets/Images/UI_AchievementIcon_3_1.png",
        (3, 2, _) => "ms-appx:///Assets/Images/UI_AchievementIcon_3_2.png",
        (3, 3, _) => "ms-appx:///Assets/Images/UI_AchievementIcon_3_3.png",
        (_, _, false) => "ms-appx:///Assets/Images/UI_AchievementIcon_1_0.png",
        (_, _, true) => "ms-appx:///Assets/Images/UI_AchievementIcon_1_1.png",
    };



}

