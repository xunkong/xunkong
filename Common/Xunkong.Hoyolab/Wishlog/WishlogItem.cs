namespace Xunkong.Hoyolab.Wishlog;

/// <summary>
/// 祈愿记录数据
/// <para/>
/// 属性已用 <see cref="JsonAttribute"/> 标注，使用 <see cref="JsonSerializer"/> 序列化时格式与官方一致
/// </summary>
public class WishlogItem : IEquatable<WishlogItem>, IJsonOnSerializing, IJsonOnDeserialized
{
    /// <summary>
    /// 用户Uid
    /// </summary>
    [JsonPropertyName("uid"), JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public int Uid { get; set; }

    /// <summary>
    /// 祈愿类型（卡池类型）
    /// </summary>
    [JsonPropertyName("gacha_type"), JsonConverter(typeof(WishTypeJsonConverter))]
    public WishType WishType { get; set; }

    /// <summary>
    /// 此值为空
    /// </summary>
    [JsonPropertyName("item_id")]
    [JsonConverter(typeof(ItemIdJsonConverter))]
    public int ItemId { get; set; }

    /// <summary>
    /// 物品数量（暂时都是1）
    /// </summary>
    [JsonPropertyName("count"), JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public int Count { get; set; } = 1;

    /// <summary>
    /// 时间
    /// </summary>
    [JsonIgnore]
    public DateTimeOffset Time { get; set; }


    /// <summary>
    /// 字符串形式的时间，时区与账号服务器所在地理位置相关，不要修改此值
    /// </summary>
    [JsonInclude, JsonPropertyName("time")]
#if NativeAOT
    public string _TimeString { get; set; }
#else
    public string _TimeString { get; private set; }
#endif

    /// <summary>
    /// 物品名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 语言（如zh-cn）
    /// </summary>
    [JsonPropertyName("lang")]
    public string Language { get; set; }

    /// <summary>
    /// 物品类型（角色、武器）
    /// </summary>
    [JsonPropertyName("item_type")]
    public string ItemType { get; set; }

    /// <summary>
    /// 星级
    /// </summary>
    [JsonPropertyName("rank_type"), JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public int RankType { get; set; }

    /// <summary>
    /// 祈愿Id，这个值很重要，全服唯一
    /// </summary>
    [JsonPropertyName("id"), JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public long Id { get; set; }


    /// <summary>
    /// 查询类型，此值为自行添加的值
    /// </summary>
    [JsonPropertyName("query_type"), JsonConverter(typeof(WishTypeJsonConverter))]
    public WishType QueryType { get; set; }


    public bool Equals(WishlogItem? other)
    {
        return (Uid, Id) == (other?.Uid, other?.Id);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Uid);
    }

    public void OnSerializing()
    {
        var offset = Uid.ToString()[0] switch
        {
            '6' => -5,
            '7' => 1,
            _ => 8,
        };
        _TimeString = Time.UtcDateTime.AddHours(offset).ToString("yyyy-MM-dd HH:mm:ss");
    }

    public void OnDeserialized()
    {
        var offset = Uid.ToString()[0] switch
        {
            '6' => -5,
            '7' => 1,
            _ => 8,
        };
        var time = DateTime.Parse(_TimeString);
        Time = new DateTimeOffset(time, TimeSpan.FromHours(offset));
    }
}
