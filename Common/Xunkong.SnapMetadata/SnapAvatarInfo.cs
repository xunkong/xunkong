namespace Xunkong.SnapMetadata;

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class BaseValue
{
    public double HpBase { get; set; }
    public double AttackBase { get; set; }
    public double DefenseBase { get; set; }
}

public class CookBonus
{
    public int OriginItemId { get; set; }
    public int ItemId { get; set; }
    public List<int> InputList { get; set; }
}

public class Costume
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsDefault { get; set; }
    public string FrontIcon { get; set; }
    public string SideIcon { get; set; }
}

public class EnergySkill
{
    public int GroupId { get; set; }
    public Proud Proud { get; set; }
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
}

public class Fetter
{
    public string Title { get; set; }
    public string Context { get; set; }
}

public class FetterInfo
{
    public string Title { get; set; }
    public string Detail { get; set; }
    public int Association { get; set; }
    public string Native { get; set; }
    public int BirthMonth { get; set; }
    public int BirthDay { get; set; }
    public string VisionBefore { get; set; }
    public string VisionAfter { get; set; }
    public string ConstellationBefore { get; set; }
    public string ConstellationAfter { get; set; }
    public string CvChinese { get; set; }
    public string CvJapanese { get; set; }
    public string CvEnglish { get; set; }
    public string CvKorean { get; set; }
    public CookBonus CookBonus { get; set; }
    public List<Fetter> Fetters { get; set; }
    public List<FetterStory> FetterStories { get; set; }
}

public class FetterStory
{
    public string Title { get; set; }
    public string Context { get; set; }
}

public class AvatarGrowCurf
{
    public int Type { get; set; }
    public int Value { get; set; }
}

public class Inherent
{
    public int GroupId { get; set; }
    public Proud Proud { get; set; }
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
}

public class Parameter
{
    public int Level { get; set; }
    public List<double> Parameters { get; set; }
}

public class Proud
{
    public List<string> Descriptions { get; set; }
    public List<Parameter> Parameters { get; set; }
}

public class SnapAvatarInfo
{
    public int Id { get; set; }
    public int PromoteId { get; set; }
    public int Sort { get; set; }
    public int Body { get; set; }
    public string Icon { get; set; }
    public string SideIcon { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime BeginTime { get; set; }
    public int Quality { get; set; }
    public int Weapon { get; set; }
    public BaseValue BaseValue { get; set; }
    public List<AvatarGrowCurf> GrowCurves { get; set; }
    public SkillDepot SkillDepot { get; set; }
    public FetterInfo FetterInfo { get; set; }
    public List<Costume> Costumes { get; set; }
    public List<int> CultivationItems { get; set; }
}

public class Skill
{
    public int GroupId { get; set; }
    public Proud Proud { get; set; }
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
}

public class SkillDepot
{
    public List<Skill> Skills { get; set; }
    public EnergySkill EnergySkill { get; set; }
    public List<Inherent> Inherents { get; set; }
    public List<Talent> Talents { get; set; }
}

public class Talent
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
}


