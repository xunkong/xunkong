using Xunkong.GenshinData;
using Xunkong.GenshinData.Character;
using Xunkong.GenshinData.Material;

namespace Xunkong.Desktop.Models;


[INotifyPropertyChanged]
public partial class PM_CharacterWiki_CharacterInfo
{

    private bool initialized;

    [ObservableProperty]
    private CharacterInfo characterInfo;

    [ObservableProperty]
    private PM_CharacterWiki_LevelProperty levelProperty;

    [ObservableProperty]
    private int targetLevel = 90;

    [ObservableProperty]
    private bool isPromoteChecked;

    [ObservableProperty]
    private List<MaterialItem>? promoteItems;

    [ObservableProperty]
    private List<PromotionCostItem>? targetLevelCostItems;

    [ObservableProperty]
    private string showGachaSplash;

    public void Initialize()
    {
        if (!initialized)
        {
            try
            {
                var levelItems = CharacterInfo.Promotions?.LastOrDefault()?.CostItems?.Select(x => x.Item);
                var talentItems = CharacterInfo.Talents?.FirstOrDefault()?.Levels?.Skip(9)?.FirstOrDefault()?.CostItems?.Select(x => x.Item);
                PromoteItems = levelItems?.Concat(talentItems ?? new List<MaterialItem>())?.Where(x => x.Id != 104319)?.DistinctBy(x => x.Id)?.ToList();
                ShowGachaSplash = CharacterInfo.GachaSplash;
                ComputeCharacterLevelProperty();
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
            var curve = CharacterInfo.Rarity == 4 ? CharacterLevelList.CharacterCurveRarity4 : CharacterLevelList.CharacterCurveRarity5;
            var hp = CharacterInfo.HpBase * curve[level - 1];
            var attack = CharacterInfo.AttackBase * curve[level - 1];
            var defense = CharacterInfo.DefenseBase * curve[level - 1];
            var otherProp = CharacterInfo?.Promotions?.FirstOrDefault()?.AddProps?.LastOrDefault()?.PropType;
            double otherPropValue = 0;
            var promoteLevel = GetPromoteLevel(level, IsPromoteChecked);
            if (promoteLevel > 0)
            {
                var addProp = CharacterInfo?.Promotions?[promoteLevel - 1]?.AddProps;
                TargetLevelCostItems = CharacterInfo?.Promotions?[promoteLevel - 1]?.CostItems;
                if (addProp != null)
                {
                    hp += addProp.FirstOrDefault(x => x.PropType == PropertyType.BaseHp)?.Value ?? 0;
                    attack += addProp.FirstOrDefault(x => x.PropType == PropertyType.BaseAttack)?.Value ?? 0;
                    defense += addProp.FirstOrDefault(x => x.PropType == PropertyType.BaseDefense)?.Value ?? 0;
                    otherPropValue = addProp.LastOrDefault()?.Value ?? 0;
                }
            }
            else
            {
                TargetLevelCostItems = null;
            }
            if (LevelProperty is null)
            {
                LevelProperty = new PM_CharacterWiki_LevelProperty
                {
                    Hp = hp,
                    Attack = attack,
                    Defense = defense,
                    OtherProperty = otherProp!,
                    OtherPropertyValue = otherPropValue,
                };
            }
            else
            {
                LevelProperty.Hp = hp;
                LevelProperty.Attack = attack;
                LevelProperty.Defense = defense;
                LevelProperty.OtherProperty = otherProp!;
                LevelProperty.OtherPropertyValue = otherPropValue;
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
public partial class PM_CharacterWiki_LevelProperty
{

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HpString))]
    private double hp;

    public string HpString => Hp.ToString("N0");

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AttackString))]
    private double attack;

    public string AttackString => Attack.ToString("N0");

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DefenseString))]
    private double defense;

    public string DefenseString => Defense.ToString("N0");

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OtherPropertyString))]
    private string otherProperty;

    public string OtherPropertyString => PropertyType.GetDescription(OtherProperty);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OtherPropertyValueString))]
    private double otherPropertyValue;

    public string OtherPropertyValueString => OtherProperty switch
    {
        PropertyType.ElementMastery => OtherPropertyValue.ToString("N0"),
        PropertyType.ChargeEfficiency => (OtherPropertyValue + 1).ToString("P1"),
        PropertyType.Critical => (OtherPropertyValue + 0.05).ToString("P1"),
        PropertyType.CriticalHurt => (OtherPropertyValue + 0.5).ToString("P1"),
        _ => OtherPropertyValue.ToString("P1"),
    };

}
