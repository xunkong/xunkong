using System.Text.Json;

namespace Xunkong.GenshinData.Achievement;

public class AchievementData
{
    [JsonIgnore]
    public int Uid { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// 当前值
    /// </summary>
    [JsonPropertyName("current")]
    public int Current { get; set; }

    /// <summary>
    /// 状态，0非法值，1未完成，2已完成，3奖励已领取
    /// </summary>
    [JsonPropertyName("status")]
    public int Status { get; set; }

    /// <summary>
    /// 完成时间
    /// </summary>
    [JsonPropertyName("timestamp")]
    [JsonConverter(typeof(TimeStampJsonConverter))]
    public DateTimeOffset FinishedTime { get; set; }

    /// <summary>
    /// 注释
    /// </summary>
    [JsonIgnore]
    public string Comment { get; set; }

    /// <summary>
    /// 最后更新时间
    /// </summary>
    [JsonIgnore]
    public DateTimeOffset LastUpdateTime { get; set; }

}


internal class TimeStampJsonConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64());
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.ToUnixTimeSeconds());
    }
}