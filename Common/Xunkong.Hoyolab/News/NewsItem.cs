namespace Xunkong.Hoyolab.News;

public class NewsItem
{

    /// <summary>
    /// 新闻内容
    /// </summary>
    [JsonPropertyName("post")]
    public NewsPost Post { get; set; }


    //[JsonPropertyName("forum")]
    //public object Forum { get; set; }

    //[JsonPropertyName("topics")]
    //public List<object> Topics { get; set; }

    //[JsonPropertyName("user")]
    //public object User { get; set; }

    //[JsonPropertyName("stat")]
    //public object Stats { get; set; }

    //[JsonPropertyName("image_list")]
    //public List<object> ImageList { get; set; }

}
