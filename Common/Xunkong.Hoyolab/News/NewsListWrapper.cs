namespace Xunkong.Hoyolab.News;


internal class NewsListWrapper
{

    /// <summary>
    /// 是否翻到结尾了
    /// </summary>
    [JsonPropertyName("is_last")]
    public bool IsLast { get; set; }

    /// <summary>
    /// 下一次请求时使用
    /// </summary>
    [JsonPropertyName("last_id")]
    public int LastId { get; set; }

    /// <summary>
    /// 新闻
    /// </summary>
    [JsonPropertyName("list")]
    public List<NewsItem> List { get; set; }

}
