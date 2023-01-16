using System.Diagnostics.CodeAnalysis;
using Xunkong.Hoyolab.SpiralAbyss;


namespace Xunkong.Desktop.Summaries;

public class Summary2022_AbyssReport
{
    public int Uid { get; set; }

    public int FirstScheduleId { get; set; }

    public string FirstScheduleString { get; set; }

    public string Str_FirstSchedule
    {
        get
        {
            if (FirstScheduleId > 37)
            {
                return $"2022年有记录的第一期是 {FirstScheduleString}，你的深境螺旋数据可能不完整。";
            }
            else
            {
                return $"2022年有记录的第一期是 {FirstScheduleString}，你的深境螺旋数据应该是完整的。";
            }
        }
    }

    public int Count_Schedule { get; set; }

    public int TotalBattle { get; set; }

    public int TotalWin { get; set; }

    public int CountStar36 { get; set; }

    public int Count_12_3 { get; set; }

    public SpiralAbyssRank MaxDamage { get; set; }

    public SpiralAbyssRank MaxTakeDamage { get; set; }

    public List<Summary2022_AbyssReport_MostUseAvatar> MostUsedAvatars { get; set; }

    public List<Summary2022_AbyssReport_MostUseAvatar> LeastUsedAvatars { get; set; }

    public List<Summary2022_AbyssReport_MostUseTeam> MostUsedTeams { get; set; }

    public List<Summary2022_AbyssReport_MostUseTeam> LeastUsedTeams { get; set; }

    public int UsedAvatarCount { get; set; }

    public int UsedAvatarCountRank5 { get; set; }

    public int UsedAvatarCountRank4 { get; set; }

    public int Floor1To8ScheduleCount { get; set; }
}


public class Summary2022_AbyssReport_MostUseAvatar
{
    public int Count { get; set; }

    public SpiralAbyssAvatar Avatar { get; set; }

}


public class Summary2022_AbyssReport_MostUseTeam
{
    public int Count { get; set; }

    public List<SpiralAbyssAvatar> Avatars { get; set; }
}


public class Summary2022_AbyssReport_TeamCompare : IEqualityComparer<SpiralAbyssBattle>
{

    public bool Equals(SpiralAbyssBattle? x, SpiralAbyssBattle? y)
    {
        var id1 = string.Join(',', x.Avatars.Select(x => x.AvatarId).Order());
        var id2 = string.Join(',', y.Avatars.Select(x => x.AvatarId).Order());
        return id1 == id2;
    }

    public int GetHashCode([DisallowNull] SpiralAbyssBattle obj)
    {
        var id = string.Join(',', obj.Avatars.Select(x => x.AvatarId).Order());
        return id.GetHashCode();
    }
}