namespace Xunkong.Core.Hoyolab
{
    /// <summary>
    /// 角色服装
    /// </summary>
    public class AvatarCostume
    {

        [JsonPropertyName("id")]
        public int Id { get; set; }


        [JsonPropertyName("name")]
        public string Name { get; set; }


        [JsonPropertyName("icon")]
        public string Icon { get; set; }

    }
}