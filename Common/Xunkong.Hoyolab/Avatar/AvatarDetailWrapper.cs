namespace Xunkong.Hoyolab.Avatar;

internal class AvatarDetailWrapper
{
    [JsonPropertyName("avatars")]
    public List<AvatarDetail> Avatars { get; set; }
}
