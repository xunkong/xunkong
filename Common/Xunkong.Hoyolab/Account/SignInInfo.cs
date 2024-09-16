namespace Xunkong.Hoyolab.Account;

public class SignInInfo
{
    /// <summary>
    /// 累积签到天数
    /// </summary>
    [JsonPropertyName("total_sign_day")]
    public int TotalSignDays { get; set; }

    /// <summary>
    /// 今天是...
    /// </summary>
    [JsonPropertyName("today"), JsonConverter(typeof(SignTodayJsonConverter))]
    public DateTime Today { get; set; }

    /// <summary>
    /// 今日是否已签到
    /// </summary>
    [JsonPropertyName("is_sign")]
    public bool IsSign { get; set; }


    [JsonPropertyName("is_sub")]
    public bool IsSub { get; set; }


    [JsonPropertyName("first_bind")]
    public bool FirstBind { get; set; }


    [JsonPropertyName("month_first")]
    public bool IsFirstDayOfMonth { get; set; }


    [JsonPropertyName("sign_cnt_missed")]
    public int MissedCount { get; set; }
}
