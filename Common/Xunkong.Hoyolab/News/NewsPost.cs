namespace Xunkong.Hoyolab.News;

/// <summary>
/// 新闻内容
/// </summary>
public class NewsPost
{

    [JsonPropertyName("post_id"), JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public int PostId { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [JsonPropertyName("subject")]
    public string Subject { get; set; }

    /// <summary>
    /// 正文（html格式）
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [JsonPropertyName("created_at"), JsonConverter(typeof(NewsTimeJsonConverter))]
    public DateTimeOffset CreateTime { get; set; }

    /// <summary>
    /// 图片，第一张为封面
    /// </summary>
    [JsonPropertyName("images")]
    public List<string> Images { get; set; }

    /// <summary>
    /// 结构化正文
    /// </summary>
    [JsonPropertyName("structured_content")]
    public string StructuredContent { get; set; }

    /// <summary>
    /// 修改时间
    /// </summary>
    [JsonPropertyName("updated_at"), JsonConverter(typeof(NewsTimeJsonConverter))]
    public DateTimeOffset UpdateTime { get; set; }

    /// <summary>
    /// 封面
    /// </summary>
    [JsonIgnore]
    public string? Cover => Images?.FirstOrDefault();

}
