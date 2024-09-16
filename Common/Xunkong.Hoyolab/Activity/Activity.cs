namespace Xunkong.Hoyolab.Activity;

/// <summary>
/// 活动
/// </summary>
public class ActivityInfo
{

    [JsonPropertyName("recommend_id")]
    public int RecommendId { get; set; }

    [JsonPropertyName("content_id")]
    public int ContentId { get; set; }

    /// <summary>
    /// 活动名称，「炉心机造」活动
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("ext")]
    public string Ext { get; set; }

    [JsonPropertyName("type")]
    public int Type { get; set; }

    /// <summary>
    /// 介绍网址
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    [JsonPropertyName("icon")]
    public string Icon { get; set; }

    /// <summary>
    /// 起止日期，2022/06/29 10:00 ~ 2022/07/11 03:59
    /// </summary>
    [JsonPropertyName("abstract")]
    public string Abstract { get; set; }

    [JsonPropertyName("article_user_name")]
    public string ArticleUserName { get; set; }

    [JsonPropertyName("avatar_url")]
    public string AvatarUrl { get; set; }

    [JsonPropertyName("article_time")]
    public string ArticleTime { get; set; }

    /// <summary>
    /// 发布时间
    /// </summary>
    [JsonPropertyName("create_time")]
    public string CreateTime { get; set; }


}
