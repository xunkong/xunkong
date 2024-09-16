namespace Xunkong.Hoyolab.Wishlog;

public class WishlogItemWiki
{

    [JsonPropertyName("game")]
    public string Game { get; set; }

    [JsonIgnore]
    public string Language { get; set; }

    [JsonPropertyName("all_avatar")]
    public List<WishlogItemInfo> AllAvatar { get; set; }

    [JsonPropertyName("all_weapon")]
    public List<WishlogItemInfo> AllWeapon { get; set; }

}
