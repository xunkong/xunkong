using Xunkong.GenshinData.Character;
using Xunkong.SnapMetadata;

namespace Xunkong.Desktop.Models;


[INotifyPropertyChanged]
public partial class PM_CharacterWiki_CharacterInfo2
{

    private bool initialized;

    [ObservableProperty]
    private SnapAvatarInfo characterInfo;


    public string BaseName => CharacterInfo.Icon.Replace("UI_AvatarIcon_", "");

    public string Birthday => $"{CharacterInfo.FetterInfo.BirthMonth}月{CharacterInfo.FetterInfo.BirthDay}日";


    [ObservableProperty]
    private PM_CharacterWiki_LevelProperty2 levelProperty;

    [ObservableProperty]
    private int targetLevel = 90;

    [ObservableProperty]
    private bool isPromoteChecked;

    [ObservableProperty]
    private List<SnapMaterial>? promoteItems;

    [ObservableProperty]
    private List<SnapAvatarPromote> avatarPromotes;

    [ObservableProperty]
    private string showGachaSplash;

    public string PromotePropString { get; set; }


    public string Card => $"UI_AvatarIcon_{BaseName}_Card";

    public string GachaSplash => $"UI_Gacha_AvatarImg_{BaseName}";

    public string NameCardBackground => $"UI_NameCardPic_{BaseName}_Alpha";




    public void Initialize()
    {
        if (!initialized)
        {
            try
            {
                ShowGachaSplash = GachaSplash;
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
            var curve = CharacterInfo.Quality == 4 ? CharacterLevelList.CharacterCurveRarity4 : CharacterLevelList.CharacterCurveRarity5;
            var hp = CharacterInfo.BaseValue.HpBase * curve[level - 1];
            var attack = CharacterInfo.BaseValue.AttackBase * curve[level - 1];
            var defense = CharacterInfo.BaseValue.DefenseBase * curve[level - 1];
            var otherProp = (FightProperty)(AvatarPromotes?.FirstOrDefault()?.AddProperties?.LastOrDefault()?.Type ?? 0);
            double otherPropValue = 0;
            var promoteLevel = GetPromoteLevel(level, IsPromoteChecked);
            if (promoteLevel > 0)
            {
                var addProp = AvatarPromotes?[promoteLevel - 1]?.AddProperties;
                if (addProp != null)
                {
                    hp += addProp.FirstOrDefault(x => x.Type == ((int)FightProperty.FIGHT_PROP_BASE_HP))?.Value ?? 0;
                    attack += addProp.FirstOrDefault(x => x.Type == ((int)FightProperty.FIGHT_PROP_BASE_ATTACK))?.Value ?? 0;
                    defense += addProp.FirstOrDefault(x => x.Type == ((int)FightProperty.FIGHT_PROP_BASE_DEFENSE))?.Value ?? 0;
                    otherPropValue = addProp.LastOrDefault()?.Value ?? 0;
                }
            }
            if (LevelProperty is null)
            {
                LevelProperty = new PM_CharacterWiki_LevelProperty2
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
public partial class PM_CharacterWiki_LevelProperty2
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
    private FightProperty otherProperty;

    public string OtherPropertyString => OtherProperty.ToDescription();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OtherPropertyValueString))]
    private double otherPropertyValue;

    public string OtherPropertyValueString => OtherProperty switch
    {
        FightProperty.FIGHT_PROP_ELEMENT_MASTERY => OtherPropertyValue.ToString("N0"),
        FightProperty.FIGHT_PROP_CHARGE_EFFICIENCY => (OtherPropertyValue + 1).ToString("P1"),
        FightProperty.FIGHT_PROP_CRITICAL => (OtherPropertyValue + 0.05).ToString("P1"),
        FightProperty.FIGHT_PROP_CRITICAL_HURT => (OtherPropertyValue + 0.5).ToString("P1"),
        _ => OtherPropertyValue.ToString("P1"),
    };

}
