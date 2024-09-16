namespace Xunkong.GenshinData.Weapon;

public class WeaponInfo
{

    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public int Rarity { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public WeaponType WeaponType { get; set; }

    public string Icon { get; set; }

    public string AwakenIcon { get; set; }

    public string GachaIcon { get; set; }

    public string Story { get; set; }

    public int SortId { get; set; }

    public List<WeaponProperty>? Properties { get; set; }

    public List<WeaponSkill>? Skills { get; set; }

    public List<WeaponPromotion>? Promotions { get; set; }

}



/// <summary>
/// 武器突破
/// </summary>
public class WeaponPromotion
{

    public int WeaponPromoteId { get; set; }

    public int PromoteLevel { get; set; }

    public int CoinCost { get; set; }

    public int UnlockMaxLevel { get; set; }

    public List<PromotionCostItem> CostItems { get; set; }

    public List<PromotionAddProperty> AddProps { get; set; }

    public int RequiredPlayerLevel { get; set; }

}


public class WeaponProperty
{
    [JsonPropertyName("propType")]
    public string PropertyType { get; set; }

    [JsonPropertyName("initValue")]
    public double InitValue { get; set; }

    [JsonPropertyName("type")]
    public string CurveType { get; set; }
}