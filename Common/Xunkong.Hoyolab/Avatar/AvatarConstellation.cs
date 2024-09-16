namespace Xunkong.Hoyolab.Avatar;

/// <summary>
/// 角色命之座
/// </summary>
public class AvatarConstellation
{

    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 名座图标（白色）
    /// <see href="https://upload-bbs.mihoyo.com/game_record/genshin/constellation_icon/UI_Talent_S_Ayaka_01.png"/>
    /// </summary>
    [JsonPropertyName("icon")]
    public string Icon { get; set; }

    /// <summary>
    /// 名字效果（内容中会带上原神特有的 color 标签）
    /// <remarks>
    /// <para>
    /// 神里绫华的普通攻击或重击对敌人造成&lt;color=#99FFFFFF&gt;冰元素伤害&lt;/color&gt;时，有50%的几率使&lt;ccolor=#FFD780FF&gt;神里流·冰华&lt;/color&gt;的冷却时间缩减0.3秒。该效果每0.1秒只能触发一次。
    /// </para>
    /// </remarks>
    /// </summary>
    [JsonPropertyName("effect")]
    public string Effect { get; set; }

    /// <summary>
    /// 是否激活
    /// </summary>
    [JsonPropertyName("is_actived")]
    public bool IsActived { get; set; }

    /// <summary>
    /// 位置 1-6
    /// </summary>
    [JsonPropertyName("pos")]
    public int Position { get; set; }

}
