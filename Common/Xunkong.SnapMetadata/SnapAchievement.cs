namespace Xunkong.SnapMetadata;


public class SnapAchievementItem
{
    public int Id { get; set; }
    public int Goal { get; set; }
    public int Order { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public FinishReward FinishReward { get; set; }
    public int Progress { get; set; }
    public int PreviousId { get; set; }
    public bool IsDeleteWatcherAfterFinish { get; set; }
    public string Version { get; set; }
}


public class FinishReward
{
    public int Id { get; set; }
    public int Count { get; set; }
}


public class SnapAchievementGoal
{
    public int Id { get; set; }
    public int Order { get; set; }
    public string Name { get; set; }
    public string Icon { get; set; }
}

