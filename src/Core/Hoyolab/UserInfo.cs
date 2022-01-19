namespace Xunkong.Core.Hoyolab
{

    /// <summary>
    /// 米游社账户信息
    /// </summary>
    [Table("Hoyolab_Users")]
    public class UserInfo
    {
        [Key]
        [JsonPropertyName("uid"), JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
        public int Uid { get; set; }


        [JsonPropertyName("nickname")]
        public string? Nickname { get; set; }


        [JsonPropertyName("introduce")]
        public string? Introduce { get; set; }


        [JsonPropertyName("avatar")]
        public string? Avatar { get; set; }


        [JsonPropertyName("gender")]
        public int Gender { get; set; }

        //[JsonPropertyName("certification")]
        //public Certification? Certification { get; set; }

        //[JsonPropertyName("level_exps")]
        //public List<LevelExp>? LevelExps { get; set; }

        //[JsonPropertyName("achieve")]
        //public Achieve? Achieve { get; set; }

        //[JsonPropertyName("community_info")]
        //public CommunityInfo? CommunityInfo { get; set; }

        /// <summary>
        /// 头像     
        /// </summary>
        [JsonPropertyName("avatar_url")]
        public string? AvatarUrl { get; set; }

        //[JsonPropertyName("certifications")]
        //public List<string>? Certifications { get; set; }

        //[JsonPropertyName("level_exp")]
        //public LevelExp? LevelExp { get; set; }

        /// <summary>
        /// 头像框    
        /// </summary>
        [JsonPropertyName("pendant")]
        public string? Pendant { get; set; }


        [JsonIgnore]
        public string? Cookie { get; set; }

    }


    internal class UserInfoDto
    {
        [JsonPropertyName("user_info")]
        public UserInfo UserInfo { get; set; }
    }
}
