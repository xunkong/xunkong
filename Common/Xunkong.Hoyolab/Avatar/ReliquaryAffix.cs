namespace Xunkong.Hoyolab.Avatar;

/// <summary>
/// 圣遗物套装效果
/// </summary>
public class ReliquaryAffix
{

    /// <summary>
    /// 激活效果所需的数量
    /// </summary>
    [JsonPropertyName("activation_number")]
    public int ActivationNumber { get; set; }

    /// <summary>
    /// 效果描述
    /// </summary>
    [JsonPropertyName("effect")]
    public string Effect { get; set; }

}
