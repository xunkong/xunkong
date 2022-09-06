using Xunkong.GenshinData.Character;
using Xunkong.GenshinData.Material;
using Xunkong.Hoyolab.Activity;

namespace Xunkong.Desktop.Models;

public class PM_GrowthSchedule_TalentMaterialGroup : List<CalendarInfo>
{

    public ContentInfo Material { get; set; }

    public List<string> Days { get; set; }


    public PM_GrowthSchedule_TalentMaterialGroup(IEnumerable<CalendarInfo> collection) : base(collection)
    {

    }


}



public class PM_GrowthSchedule_BossMaterialGroup : List<CharacterInfo>
{
    public MaterialItem Material { get; set; }

    public PM_GrowthSchedule_BossMaterialGroup(IEnumerable<CharacterInfo> collection) : base(collection)
    {

    }
}