namespace Xunkong.Hoyolab.Account;

internal class GenshinRoleInfoWrapper
{
    [JsonPropertyName("list")]
    public List<GenshinRoleInfo>? List { get; set; }
}
