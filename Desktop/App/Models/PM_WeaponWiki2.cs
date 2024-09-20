using Xunkong.GenshinData.Weapon;
using Xunkong.SnapMetadata;

namespace Xunkong.Desktop.Models;


[INotifyPropertyChanged]
public partial class PM_WeaponWiki_WeaponInfo2
{

    private bool initialized;

    [ObservableProperty]
    private SnapWeaponInfo weaponInfo;

    public string BaseName => WeaponInfo.Icon.Replace("UI_EquipIcon_", "");

    [ObservableProperty]
    private PM_WeaponWiki_LevelProperty2 levelProperty;

    [ObservableProperty]
    private int maxLevel;

    [ObservableProperty]
    private int targetLevel;

    [ObservableProperty]
    private bool isPromoteChecked;

    [ObservableProperty]
    private List<SnapMaterial>? promoteItems;

    [ObservableProperty]
    private List<SnapPromote> weaponPromotes;


    public string GachaIcon => $"UI_Gacha_EquipIcon_{BaseName}";

    public string PromotePropString { get; set; }

    public void Initialize()
    {
        if (!initialized)
        {
            try
            {
                MaxLevel = WeaponInfo.RankLevel > 2 ? 90 : 70;
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
            var primaryProp = WeaponInfo.GrowCurves?.FirstOrDefault()?.Type;
            var primaryPropValue = WeaponInfo.GrowCurves?.FirstOrDefault()?.InitValue ?? 0;
            var primaryCurve = WeaponLevelList.CurveTypeToList(((GrowCurveType)WeaponInfo.GrowCurves?.FirstOrDefault()?.Value!).ToString());
            primaryPropValue *= primaryCurve[level - 1];

            var secondaryProp = WeaponInfo.GrowCurves?.LastOrDefault()?.Type;
            double seconaryPropValue = 0;
            if (secondaryProp != primaryProp)
            {
                seconaryPropValue = WeaponInfo.GrowCurves?.LastOrDefault()?.InitValue ?? 0;
                var secondaryCurve = WeaponLevelList.CurveTypeToList(((GrowCurveType)WeaponInfo.GrowCurves?.LastOrDefault()?.Value!).ToString());
                seconaryPropValue *= secondaryCurve[Math.Clamp(level / 5 * 5 - 1, 1, 90)];
            }
            else
            {
                secondaryProp = null;
            }

            var promoteLevel = GetPromoteLevel(level, IsPromoteChecked);
            if (promoteLevel > 0)
            {
                var addProp = WeaponPromotes?[promoteLevel - 1]?.AddProperties?.FirstOrDefault();
                if (addProp != null)
                {
                    primaryPropValue += addProp.Value;
                }
            }
            if (LevelProperty is null)
            {
                LevelProperty = new PM_WeaponWiki_LevelProperty2
                {
                    PrimaryProp = primaryProp.Value,
                    PrimaryPropValue = primaryPropValue,
                    SecondaryProp = secondaryProp,
                    SecondaryPropValue = seconaryPropValue,
                };
            }
            else
            {
                LevelProperty.PrimaryProp = primaryProp.Value;
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
public partial class PM_WeaponWiki_LevelProperty2
{

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PrimaryPropString))]
    private int primaryProp;

    public string PrimaryPropString => ((FightProperty)PrimaryProp).ToDescription();


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PrimaryPropValueString))]
    private double primaryPropValue;


    public string PrimaryPropValueString => PrimaryPropValue.ToString("N0");


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SecondaryPropString))]
    private int? secondaryProp;

    public string SecondaryPropString => SecondaryProp.HasValue ? ((FightProperty)SecondaryProp).ToDescription() : "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SecondaryPropValueString))]
    private double secondaryPropValue;


    public string SecondaryPropValueString => SecondaryProp switch
    {
        (int)FightProperty.FIGHT_PROP_ELEMENT_MASTERY => SecondaryPropValue.ToString("N0"),
        _ => SecondaryPropValue.ToString("P1"),
    };


}
