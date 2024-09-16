namespace Xunkong.Hoyolab.Avatar;

public class AvatarDetail : AvatarInfo
{

    /// <summary>
    /// 半身肖像
    /// <see href="https://upload-bbs.mihoyo.com/game_record/genshin/character_icon/UI_AvatarIcon_Ayaka.png"/>
    /// </summary>
    [JsonPropertyName("icon")]
    public string Icon { get; set; }

    /// <summary>
    /// 半透明全身图片
    /// <see href="https://upload-bbs.mihoyo.com/game_record/genshin/character_image/UI_AvatarIcon_Ayaka@2x.png"/>
    /// </summary>
    [JsonPropertyName("image")]
    public new string Image { get; set; }

    /// <summary>
    /// 已装备武器
    /// </summary>
    [JsonPropertyName("weapon")]
    public AvatarWeapon Weapon { get; set; }

    /// <summary>
    /// 已装备圣遗物
    /// </summary>
    [JsonPropertyName("reliquaries")]
    public List<AvatarReliquary> Reliquaries { get; set; }

    /// <summary>
    /// 命之座
    /// </summary>
    [JsonPropertyName("constellations")]
    public List<AvatarConstellation> Constellations { get; set; }

    /// <summary>
    /// 已拥有服装
    /// </summary>
    [JsonPropertyName("costumes")]
    public List<AvatarCostume> Costumes { get; set; }

}
