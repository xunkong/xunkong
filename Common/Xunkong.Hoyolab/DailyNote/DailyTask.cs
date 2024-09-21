namespace Xunkong.Hoyolab.DailyNote;

public class DailyTask
{

    [JsonPropertyName("total_num")]
    public int TotalNum { get; set; }

    [JsonPropertyName("finished_num")]
    public int FinishedNum { get; set; }

    /// <summary>
    /// 额外奖励已领取
    /// </summary>
    [JsonPropertyName("is_extra_task_reward_received")]
    public bool IsExtraTaskRewardReceived { get; set; }

    /// <summary>
    /// 委托任务
    /// </summary>
    [JsonPropertyName("task_rewards")]
    public List<TaskReward> TaskRewards { get; set; }

    /// <summary>
    /// 历练点
    /// </summary>
    [JsonPropertyName("attendance_rewards")]
    public List<AttendanceReward> AttendanceRewards { get; set; }

    [JsonPropertyName("attendance_visible")]
    public bool AttendanceVisible { get; set; }

    /// <summary>
    /// 长效历练点
    /// </summary>
    [JsonPropertyName("stored_attendance")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public double StoredAttendance { get; set; }

    /// <summary>
    /// 长效历练点重置时间，单位：秒
    /// </summary>
    [JsonPropertyName("stored_attendance_refresh_countdown")]
    public int StoredAttendanceRefreshCountdown { get; set; }

}


public class AttendanceReward
{
    /// <summary>
    /// AttendanceRewardStatusInvalid
    /// AttendanceRewardStatusTakenAward
    /// AttendanceRewardStatusWaitTaken
    /// AttendanceRewardStatusUnfinished
    /// AttendanceRewardStatusFinishedNonReward
    /// AttendanceRewardStatusForbid
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; }

    /// <summary>
    /// 历练点进度，满值2000
    /// </summary>
    [JsonPropertyName("progress")]
    public int Progress { get; set; }
}


public class TaskReward
{
    /// <summary>
    /// TaskRewardStatusInvalid
    /// TaskRewardStatusTakenAward
    /// TaskRewardStatusFinished
    /// TaskRewardStatusUnfinished
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; }
}