using Xunkong.GenshinData.Material;

namespace Xunkong.GenshinData.Character;

public class CharacterInfo
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public int Rarity { get; set; }

    /// <summary>
    /// 性别，男性0，女性1
    /// </summary>
    public int Gender { get; set; }

    /// <summary>
    /// 所属势力
    /// </summary>
    public string Affiliation { get; set; }

    /// <summary>
    /// 命座名称
    /// </summary>
    public string ConstllationName { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ElementType Element { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public WeaponType WeaponType { get; set; }

    /// <summary>
    /// MM/dd
    /// </summary>
    public string Birthday { get; set; }

    public string Card { get; set; }

    public string FaceIcon { get; set; }

    public string SideIcon { get; set; }

    public string GachaCard { get; set; }

    public string GachaSplash { get; set; }

    public string CvChinese { get; set; }

    public string CvJapanese { get; set; }

    public string CvEnglish { get; set; }

    public string CvKorean { get; set; }


    /// <summary>
    /// 基础生命值
    /// </summary>
    public double HpBase { get; set; }

    /// <summary>
    /// 基础攻击力
    /// </summary>
    public double AttackBase { get; set; }

    /// <summary>
    /// 基础防御力
    /// </summary>
    public double DefenseBase { get; set; }



    /// <summary>
    /// 排序
    /// </summary>
    public int SortId { get; set; }

    /// <summary>
    /// 可使用时间
    /// </summary>
    public DateTime BeginTime { get; set; }


    /// <summary>
    /// 天赋
    /// </summary>
    public List<CharacterTalent>? Talents { get; set; }


    /// <summary>
    /// 命之座
    /// </summary>
    public List<CharacterConstellation>? Constellations { get; set; }

    /// <summary>
    /// 角色突破
    /// </summary>
    public List<CharacterPromotion>? Promotions { get; set; }

    /// <summary>
    /// 名片
    /// </summary>
    public NameCard? NameCard { get; set; }


    public Food? Food { get; set; }


    public List<CharacterOutfit>? Outfits { get; set; }


    public List<CharacterStory>? Stories { get; set; }

    public List<CharacterVoice>? Voices { get; set; }


}
