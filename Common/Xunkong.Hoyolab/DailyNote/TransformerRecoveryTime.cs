namespace Xunkong.Hoyolab.DailyNote;

/// <summary>
/// 参量质变仪恢复时间
/// <para>时间四值中仅有 <see cref="Day"/> 或 <see cref="Hour"/> 有值</para>
/// </summary>
public class TransformerRecoveryTime
{
    [JsonPropertyName("Day")]
    public int Day { get; set; }

    [JsonPropertyName("Hour")]
    public int Hour { get; set; }

    [JsonPropertyName("Minute")]
    public int Minute { get; set; }

    [JsonPropertyName("Second")]
    public int Second { get; set; }

    /// <summary>
    /// 是否可再次使用
    /// </summary>
    [JsonPropertyName("reached")]
    public bool Reached { get; set; }
}
