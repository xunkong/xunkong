using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

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
    public string? FrontIcon { get; set; }
    public string? SideIcon { get; set; }

    [JsonIgnore]
    public string? BaseName => FrontIcon?.Replace("UI_AvatarIcon_", "");
    [JsonIgnore]
    public string? Card => IsDefault ? null : $"UI_AvatarIcon_{BaseName}_Card";
    [JsonIgnore]
    public string? GachaSplash => IsDefault ? null : $"UI_Costume_{BaseName}";
}

public class Fetter
{
    public string Title { get; set; }
    public string Context { get; set; }
}

public class FetterInfo : IJsonOnDeserialized
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

    public void OnDeserialized()
    {
        if (string.IsNullOrWhiteSpace(ConstellationAfter))
        {
            ConstellationAfter = ConstellationBefore;
        }
    }
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

    [JsonIgnore]
    public List<string> ShownParameters { get; set; }
}

public class Proud : IJsonOnDeserialized
{
    public List<string> Descriptions { get; set; }
    public List<Parameter> Parameters { get; set; }

    [JsonIgnore]
    public bool HasValue => Descriptions.Count > 0;

    [JsonIgnore]
    public List<string> ShownDescriptions { get; set; }

    public void OnDeserialized()
    {
        try
        {
            ShownDescriptions = Descriptions.Select(x => x[..x.IndexOf('|')]).ToList();
            foreach (var param in Parameters)
            {
                List<string> models = Descriptions.Select(x => x[(x.IndexOf('|') + 1)..]).ToList();
                param.ShownParameters = models.Select(x =>
                {
                    string result = x;
                    var matches = Regex.Matches(x, @"{param(\d):([^}]+)}");
                    foreach (Match match in matches)
                    {
                        var id = match.Groups[1].Value;
                        var p = match.Groups[2].Value;
                        double value = 0;
                        if (int.TryParse(id, out int i) && i > 0)
                        {
                            value = param.Parameters[i - 1];
                        }
                        string v = "";
                        if (p is "I")
                        {
                            v = value.ToString("N0");
                        }
                        else if (p.Contains("P") && p.Contains("F"))
                        {
                            v = value.ToString($"P{p.Replace("F", "").Replace("P", "")}");
                        }
                        else
                        {
                            v = value.ToString(p);
                        }
                        result = result.Replace(match.Value, v);
                    }
                    return result;
                }).ToList();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }
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
    public Skill EnergySkill { get; set; }
    public List<Skill> Inherents { get; set; }
    public List<Talent> Talents { get; set; }

    [JsonIgnore]
    public List<Skill> AllSkills => Skills.Concat([EnergySkill]).Concat(Inherents).ToList();
}

public class Talent
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
}


