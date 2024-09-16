namespace Xunkong.GenshinData;

/// <summary>
/// 任务、成就完成时的奖励
/// </summary>
public class Reward
{
    public int RewardId { get; set; }

    public List<RewardItem> RewardItemList { get; set; }

}

/// <summary>
/// 奖励物品
/// </summary>
public class RewardItem
{
    public int ItemId { get; set; }

    public int ItemCount { get; set; }
}
