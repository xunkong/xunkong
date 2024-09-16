namespace Xunkong.Hoyolab.Activity;

public class Announcement
{

    [JsonPropertyName("ann_id")]
    public int AnnId { get; set; }

    /// <summary>
    /// 完整标题
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; }

    /// <summary>
    /// 字数少的标题
    /// </summary>
    [JsonPropertyName("subtitle")]
    public string Subtitle { get; set; }

    /// <summary>
    /// 图片，可能为空
    /// </summary>
    [JsonPropertyName("banner")]
    public string? Banner { get; set; }

    /// <summary>
    /// 正文，特殊的 HTML 格式
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; }

    /// <summary>
    /// 游戏公告、活动公告
    /// </summary>
    [JsonPropertyName("type_label")]
    public string TypeLabel { get; set; }

    /// <summary>
    /// 重要、活动
    /// </summary>
    [JsonPropertyName("tag_label")]
    public string TagLabel { get; set; }

    /// <summary>
    /// 左侧列表的小图标
    /// </summary>
    [JsonPropertyName("tag_icon")]
    public string TagIcon { get; set; }

    [JsonPropertyName("login_alert")]
    public int LoginAlert { get; set; }

    /// <summary>
    /// zh-cn
    /// </summary>
    [JsonPropertyName("lang")]
    public string Lang { get; set; }

    /// <summary>
    /// 开始时间（不准确，有提前）
    /// </summary>
    [JsonPropertyName("start_time")]
    public string StartTime { get; set; }

    /// <summary>
    /// 结束时间（准确，没时区）
    /// </summary>
    [JsonPropertyName("end_time")]
    public string EndTime { get; set; }

    /// <summary>
    /// 1：活动公告、2：游戏公告
    /// </summary>
    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("remind")]
    public int Remind { get; set; }

    [JsonPropertyName("alert")]
    public int Alert { get; set; }

    /// <summary>
    /// 没用
    /// </summary>
    [JsonPropertyName("tag_start_time")]
    public string TagStartTime { get; set; }

    /// <summary>
    /// 没用
    /// </summary>
    [JsonPropertyName("tag_end_time")]
    public string TagEndTime { get; set; }

    [JsonPropertyName("remind_ver")]
    public int RemindVer { get; set; }

    [JsonPropertyName("has_content")]
    public bool HasContent { get; set; }

    [JsonPropertyName("extra_remind")]
    public int ExtraRemind { get; set; }


    /// <summary>
    /// 即将结束
    /// </summary>
    [JsonIgnore]
    public bool IsFinishing
    {
        get
        {
            if (DateTimeOffset.TryParse(EndTime + "+08:00", out var result))
            {
                var remainTime = result - DateTimeOffset.Now;
                if (remainTime > TimeSpan.Zero && remainTime < TimeSpan.FromDays(3))
                {
                    return true;
                }
            }
            return false;
        }
    }

    [JsonIgnore]
    public string? RemainTimeString
    {
        get
        {
            if (Type == 1)
            {
                if (DateTimeOffset.TryParse(EndTime + "+08:00", out var result))
                {
                    var remainTime = result - DateTimeOffset.Now;
                    if (remainTime.Days > 0)
                    {
                        return $"{remainTime.Days}天{remainTime.Hours}小时后结束";
                    }
                    else
                    {
                        return $"{remainTime.Hours}小时{remainTime.Minutes}分钟后结束";
                    }
                }
            }
            return null;
        }
    }

}



internal class AnnouncementContent
{
    [JsonPropertyName("ann_id")]
    public int AnnId { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("subtitle")]
    public string Subtitle { get; set; }

    [JsonPropertyName("banner")]
    public string Banner { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; }

    [JsonPropertyName("lang")]
    public string Lang { get; set; }
}