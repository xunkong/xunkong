namespace Xunkong.Core.Hoyolab
{

    /// <summary>
    /// 角色命之座
    /// </summary>
    public class AvatarConstellation
    {

        [JsonPropertyName("id")]
        public int Id { get; set; }


        [JsonPropertyName("name")]
        public string Name { get; set; }


        [JsonPropertyName("icon")]
        public string Icon { get; set; }


        [JsonPropertyName("effect")]
        public string Effect { get; set; }


        [JsonPropertyName("is_actived")]
        public bool IsActived { get; set; }


        [JsonPropertyName("pos")]
        public int Position { get; set; }


    }
}