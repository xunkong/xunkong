namespace Xunkong.Hoyolab.Account;

internal class HoyolabUserInfoWrapper
{
    [JsonPropertyName("user_info")]
    public HoyolabUserInfo HoyolabUserInfo { get; set; }
}
