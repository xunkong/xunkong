namespace Xunkong.Hoyolab.GameRecord;

/// <summary>
/// 贡品类型奖励信息
/// </summary>
public class WorldExplorationOffering
{

    [JsonPropertyName("name")]
    public string Name { get; set; }


    [JsonPropertyName("level")]
    public int Level { get; set; }


    [JsonPropertyName("icon")]
    public string Icon { get; set; }

}
