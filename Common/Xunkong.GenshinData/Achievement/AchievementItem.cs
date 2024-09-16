namespace Xunkong.GenshinData.Achievement;

public class AchievementItem
{

    public int Id { get; set; }

    public int GoalId { get; set; }

    public int OrderId { get; set; }

    public int PreStageAchievementId { get; set; }

    /// <summary>
    /// 隐藏成就
    /// </summary>
    public bool IsHide { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public int FinishRewardId { get; set; }

    public int RewardCount { get; set; }

    /// <summary>
    /// 完成后删除进度
    /// </summary>
    public bool IsDeleteWatcherAfterFinish { get; set; }

    public TriggerConfig TriggerConfig { get; set; }

    /// <summary>
    /// 完成此成就所需条件的数值
    /// </summary>
    public int Progress { get; set; }


    public string Version { get; set; }
}
