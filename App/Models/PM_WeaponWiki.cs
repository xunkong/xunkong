using Xunkong.GenshinData;
using Xunkong.GenshinData.Material;
using Xunkong.GenshinData.Weapon;

namespace Xunkong.Desktop.Models;


[INotifyPropertyChanged]
public partial class PM_WeaponWiki_WeaponInfo
{

    private bool initialized;

    [ObservableProperty]
    private WeaponInfo weaponInfo;

    [ObservableProperty]
    private PM_WeaponWiki_LevelProperty levelProperty;

    [ObservableProperty]
    private int maxLevel;

    [ObservableProperty]
    private int targetLevel;

    [ObservableProperty]
    private bool isPromoteChecked;

    [ObservableProperty]
    private List<MaterialItem>? promoteItems;

    [ObservableProperty]
    private List<PromotionCostItem>? targetLevelCostItems;

    public string PromotePropString { get; set; }

    public void Initialize()
    {
        if (!initialized)
        {
            try
            {
                PromoteItems = WeaponInfo.Promotions?.LastOrDefault()?.CostItems?.Select(x => x.Item).ToList();
                MaxLevel = WeaponInfo.Rarity > 2 ? 90 : 70;
                TargetLevel = MaxLevel;
                initialized = true;
            }
            catch (Exception ex)
            {

            }
        }
    }


    partial void OnTargetLevelChanged(int value)
    {
        ComputeCharacterLevelProperty();
    }


    partial void OnIsPromoteCheckedChanged(bool value)
    {
        ComputeCharacterLevelProperty();
    }


    public void ComputeCharacterLevelProperty()
    {
        try
        {
            var level = TargetLevel;
            var primaryProp = WeaponInfo.Properties?.FirstOrDefault()?.PropertyType;
            var primaryPropValue = WeaponInfo.Properties?.FirstOrDefault()?.InitValue ?? 0;
            var primaryCurve = WeaponLevelList.CurveTypeToList(WeaponInfo.Properties?.FirstOrDefault()?.CurveType!);
            primaryPropValue *= primaryCurve[level - 1];

            var secondaryProp = WeaponInfo.Properties?.LastOrDefault()?.PropertyType;
            double seconaryPropValue = 0;
            if (secondaryProp != primaryProp)
            {
                seconaryPropValue = WeaponInfo.Properties?.LastOrDefault()?.InitValue ?? 0;
                var secondaryCurve = WeaponLevelList.CurveTypeToList(WeaponInfo.Properties?.LastOrDefault()?.CurveType!);
                seconaryPropValue *= secondaryCurve[Math.Clamp(level / 5 * 5 - 1, 1, 90)];
            }
            else
            {
                secondaryProp = null;
            }

            var promoteLevel = GetPromoteLevel(level, IsPromoteChecked);
            if (promoteLevel > 0)
            {
                var addProp = WeaponInfo.Promotions?[promoteLevel - 1]?.AddProps?.FirstOrDefault();
                TargetLevelCostItems = WeaponInfo?.Promotions?[promoteLevel - 1]?.CostItems;
                if (addProp != null)
                {
                    primaryPropValue += addProp.Value;
                }
            }
            else
            {
                TargetLevelCostItems = null;
            }
            if (LevelProperty is null)
            {
                LevelProperty = new PM_WeaponWiki_LevelProperty
                {
                    PrimaryProp = primaryProp!,
                    PrimaryPropValue = primaryPropValue,
                    SecondaryProp = secondaryProp,
                    SecondaryPropValue = seconaryPropValue,
                };
            }
            else
            {
                LevelProperty.PrimaryProp = primaryProp!;
                LevelProperty.PrimaryPropValue = primaryPropValue;
                LevelProperty.SecondaryProp = secondaryProp;
                LevelProperty.SecondaryPropValue = seconaryPropValue;
            }
        }
        catch (Exception ex)
        {

        }
    }

    private static int GetPromoteLevel(int level, bool isPromote) => (level, isPromote) switch
    {
        ( < 20, _) => 0,
        (20, false) => 0,
        (20, true) => 1,
        ( < 40, _) => 1,
        (40, false) => 1,
        (40, true) => 2,
        ( < 50, _) => 2,
        (50, false) => 2,
        (50, true) => 3,
        ( < 60, _) => 3,
        (60, false) => 3,
        (60, true) => 4,
        ( < 70, _) => 4,
        (70, false) => 4,
        (70, true) => 5,
        ( < 80, _) => 5,
        (80, false) => 5,
        (80, true) => 6,
        ( <= 90, _) => 6,
        _ => 0,
    };

}





[INotifyPropertyChanged]
public partial class PM_WeaponWiki_LevelProperty
{

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PrimaryPropString))]
    private string primaryProp;

    public string PrimaryPropString => PropertyType.GetDescription(PrimaryProp);


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PrimaryPropValueString))]
    private double primaryPropValue;


    public string PrimaryPropValueString => PrimaryPropValue.ToString("N0");


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SecondaryPropString))]
    private string? secondaryProp;

    public string SecondaryPropString => PropertyType.GetDescription(SecondaryProp);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SecondaryPropValueString))]
    private double secondaryPropValue;


    public string SecondaryPropValueString => SecondaryProp switch
    {
        PropertyType.ElementMastery => SecondaryPropValue.ToString("N0"),
        _ => SecondaryPropValue.ToString("P1"),
    };


}
