namespace Xunkong.Hoyolab.Avatar;

/// <summary>
/// 角色服装
/// </summary>
public class AvatarCostume
{

    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// 服装名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 服装图片
    /// <see href="https://upload-bbs.mihoyo.com/game_record/genshin/costume/UI_AvatarIcon_BarbaraCostumeSummertime@2x.png"/>
    /// </summary>
    [JsonPropertyName("icon")]
    public string Icon { get; set; }

}
