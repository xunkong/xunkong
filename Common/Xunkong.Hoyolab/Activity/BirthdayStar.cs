namespace Xunkong.Hoyolab.Activity;

/// <summary>
/// 留影叙佳期，今日生日
/// </summary>
public class BirthdayStarIndex
{

    [JsonPropertyName("nick_name")]
    public string NickName { get; set; }

    [JsonPropertyName("uid")]
    public int Uid { get; set; }

    [JsonPropertyName("region")]
    public string Region { get; set; }

    /// <summary>
    /// 过生日的角色
    /// </summary>
    [JsonPropertyName("role")]
    public List<BirthdayStarIndexRole> Role { get; set; }

    [JsonPropertyName("draw_notice")]
    public bool DrawNotice { get; set; }

    [JsonPropertyName("CurrentTime")]
    public string CurrentTime { get; set; }

    [JsonPropertyName("gender")]
    public int Gender { get; set; }

    [JsonPropertyName("is_show_remind")]
    public bool IsShowRemind { get; set; }

}


/// <summary>
/// 留影叙佳期今天过生日的角色
/// </summary>
public class BirthdayStarIndexRole
{
    [JsonPropertyName("role_id")]
    public int RoleId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("jump_type")]
    public string JumpType { get; set; }

    [JsonPropertyName("jump_target")]
    public string JumpTarget { get; set; }

    [JsonPropertyName("jump_start_time")]
    public string JumpStartTime { get; set; }

    [JsonPropertyName("jump_end_time")]
    public string JumpEndTime { get; set; }

    [JsonPropertyName("role_gender")]
    public int RoleGender { get; set; }

    [JsonPropertyName("take_picture")]
    public string TakePicture { get; set; }

    [JsonPropertyName("gal_xml")]
    public string GalXml { get; set; }

    [JsonPropertyName("gal_resource")]
    public string GalResource { get; set; }

    /// <summary>
    /// 已获取相片
    /// </summary>
    [JsonPropertyName("is_partake")]
    public bool IsPartake { get; set; }

    [JsonPropertyName("bgm")]
    public string Bgm { get; set; }
}


/// <summary>
/// 留影叙佳期相册
/// </summary>
public class BirthdayStarDrawCollection
{
    [JsonPropertyName("my_draws")]
    public List<BirthdayStarDrawItem> MyDraws { get; set; }

    [JsonPropertyName("current_page")]
    public int CurrentPage { get; set; }

    [JsonPropertyName("total_page")]
    public int TotalPage { get; set; }

    [JsonPropertyName("draw_notice")]
    public bool DrawNotice { get; set; }

}


/// <summary>
/// 留影叙佳期相片
/// </summary>
public class BirthdayStarDrawItem
{
    /// <summary>
    /// normal / miss
    /// </summary>
    [JsonPropertyName("draw_status")]
    public string DrawStatus { get; set; }

    /// <summary>
    /// 已获得的相片（错过了生日此值为空
    /// </summary>
    [JsonPropertyName("take_picture")]
    public string TakePicture { get; set; }

    /// <summary>
    /// 模糊的相片
    /// </summary>
    [JsonPropertyName("unread_picture")]
    public string UnreadPicture { get; set; }

    [JsonPropertyName("word_text")]
    public string WordText { get; set; }

    [JsonPropertyName("year")]
    public int Year { get; set; }

    /// <summary>
    /// MM/dd
    /// </summary>
    [JsonPropertyName("birthday")]
    public string Birthday { get; set; }

    [JsonPropertyName("is_new")]
    public bool IsNew { get; set; }

    [JsonPropertyName("role_id")]
    public int RoleId { get; set; }

    [JsonPropertyName("gal_xml")]
    public string GalXml { get; set; }

    [JsonPropertyName("gal_resource")]
    public string GalResource { get; set; }

    /// <summary>
    /// 收藏
    /// </summary>
    [JsonPropertyName("is_collected")]
    public bool IsCollected { get; set; }

    [JsonPropertyName("op_id")]
    public int OpId { get; set; }

}