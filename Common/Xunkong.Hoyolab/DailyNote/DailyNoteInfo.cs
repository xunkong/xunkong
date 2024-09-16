namespace Xunkong.Hoyolab.DailyNote;

/// <summary>
/// 实时便笺
/// </summary>
public class DailyNoteInfo
{

    [JsonIgnore]
    public int Id { get; set; }

    [JsonIgnore]
    public int Uid { get; set; }

    [JsonIgnore]
    public string Nickname { get; set; }

    /// <summary>
    /// 获取实时便笺时的时间
    /// </summary>
    public DateTimeOffset UpdateTime { get; init; } = DateTimeOffset.Now;

    /// <summary>
    /// 当前树脂
    /// </summary>
    [JsonPropertyName("current_resin")]
    public int CurrentResin { get; set; }

    /// <summary>
    /// 最大树脂
    /// </summary>
    [JsonPropertyName("max_resin")]
    public int MaxResin { get; set; }

    /// <summary>
    /// 树脂剩余恢复时间
    /// </summary>
    [JsonPropertyName("resin_recovery_time"), JsonConverter(typeof(RecoveryTimeJsonConverter))]
    public TimeSpan ResinRecoveryTime { get; set; }

    /// <summary>
    /// 树脂是否恢复满
    /// </summary>
    [JsonIgnore]
    public bool IsResinFull => ResinRecoveryTime == TimeSpan.Zero;

    /// <summary>
    /// 树脂恢复满的时刻
    /// </summary>
    [JsonIgnore]
    public DateTimeOffset ResinFullTime => UpdateTime + ResinRecoveryTime;

    /// <summary>
    /// 委托完成数
    /// </summary>
    [JsonPropertyName("finished_task_num")]
    public int FinishedTaskNumber { get; set; }

    /// <summary>
    /// 委托总数
    /// </summary>
    [JsonPropertyName("total_task_num")]
    public int TotalTaskNumber { get; set; }

    /// <summary>
    /// 4次委托额外奖励是否领取
    /// </summary>
    [JsonPropertyName("is_extra_task_reward_received")]
    public bool IsExtraTaskRewardReceived { get; set; }

    /// <summary>
    /// 剩余周本树脂减半次数
    /// </summary>
    [JsonPropertyName("remain_resin_discount_num")]
    public int RemainResinDiscountNumber { get; set; }

    /// <summary>
    /// 周本树脂减半总次数
    /// </summary>
    [JsonPropertyName("resin_discount_num_limit")]
    public int ResinDiscountLimitedNumber { get; set; }

    /// <summary>
    /// 当前派遣数
    /// </summary>
    [JsonPropertyName("current_expedition_num")]
    public int CurrentExpeditionNumber { get; set; }

    /// <summary>
    /// 已完成派遣数
    /// </summary>
    [JsonIgnore]
    public int FinishedExpeditionNumber => Expeditions?.Count(x => x.IsFinished) ?? 0;

    /// <summary>
    /// 最大派遣数
    /// </summary>
    [JsonPropertyName("max_expedition_num")]
    public int MaxExpeditionNumber { get; set; }

    /// <summary>
    /// 探索派遣
    /// </summary>
    [JsonPropertyName("expeditions")]
    public List<Expedition> Expeditions { get; set; }

    /// <summary>
    /// 当前洞天宝钱
    /// </summary>
    [JsonPropertyName("current_home_coin")]
    public int CurrentHomeCoin { get; set; }

    /// <summary>
    /// 最大洞天宝钱
    /// </summary>
    [JsonPropertyName("max_home_coin")]
    public int MaxHomeCoin { get; set; }

    /// <summary>
    /// 洞天宝钱剩余恢复时间
    /// </summary>
    [JsonPropertyName("home_coin_recovery_time"), JsonConverter(typeof(RecoveryTimeJsonConverter))]
    public TimeSpan HomeCoinRecoveryTime { get; set; }

    /// <summary>
    /// 参量质变仪
    /// </summary>
    [JsonPropertyName("transformer")]
    public Transformer Transformer { get; set; }

    /// <summary>
    /// 洞天宝钱是否已满
    /// </summary>
    [JsonIgnore]
    public bool IsHomeCoinFull => HomeCoinRecoveryTime == TimeSpan.Zero;

    /// <summary>
    /// 洞天宝钱攒满时刻
    /// </summary>
    [JsonIgnore]
    public DateTimeOffset HomeCoinFullTime => UpdateTime + HomeCoinRecoveryTime;

}
