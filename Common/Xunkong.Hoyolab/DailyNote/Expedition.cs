namespace Xunkong.Hoyolab.DailyNote;

/// <summary>
/// 探索派遣
/// </summary>
public class Expedition
{
    /// <summary>
    /// 角色侧面图
    /// </summary>
    [JsonPropertyName("avatar_side_icon")]
    public string AvatarSideIcon { get; set; }

    /// <summary>
    /// 状态 Ongoing:派遣中 Finished:已完成
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonIgnore]
    public DateTimeOffset NowTime { get; init; } = DateTimeOffset.Now;

    /// <summary>
    /// 剩余时间
    /// </summary>
    [JsonPropertyName("remained_time"), JsonConverter(typeof(RecoveryTimeJsonConverter))]
    public TimeSpan RemainedTime { get; set; }

    /// <summary>
    /// 探索派遣是否完成
    /// </summary>
    [JsonIgnore]
    public bool IsFinished => RemainedTime == TimeSpan.Zero;

    /// <summary>
    /// 完成时刻
    /// </summary>
    [JsonIgnore]
    public DateTimeOffset FinishedTime => NowTime + RemainedTime;

}
