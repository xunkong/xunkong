namespace Xunkong.Core.Hoyolab
{
    public class AvatarDetail : AvatarInfo
    {

        [JsonPropertyName("icon")]
        public string Icon { get; set; }


        [JsonPropertyName("weapon")]
        public AvatarWeapon Weapon { get; set; }


        [JsonPropertyName("reliquaries")]
        public List<AvatarReliquary> Reliquaries { get; set; }


        [JsonPropertyName("constellations")]
        public List<AvatarConstellation> Constellations { get; set; }


        [JsonPropertyName("costumes")]
        public List<AvatarCostume> Costumes { get; set; }

    }


    internal class AvatarDetailResponseData
    {
        [JsonPropertyName("avatars")]
        public List<AvatarDetail> Avatars { get; set; }
    }
}
