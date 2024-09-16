using Xunkong.GenshinData.Material;

namespace Xunkong.GenshinData.Achievement;

public class AchievementGoal
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public string Name { get; set; }

    public int FinishRewardId { get; set; }

    public string IconPath { get; set; }

    public string SmallIcon { get; set; }

    public NameCard? RewardNameCard { get; set; }
}
