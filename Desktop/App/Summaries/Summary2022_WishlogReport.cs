using Xunkong.Hoyolab.Wishlog;

namespace Xunkong.Desktop.Summaries;

public class Summary2022_WishlogReport
{
    public int Uid { get; set; }

    public bool IsFirstTimeIn2022 { get; set; }

    public DateTimeOffset FirstTime { get; set; }

    public string Str_FirstTime
    {
        get
        {
            if (IsFirstTimeIn2022)
            {
                return $"第一条记录的抽取时间是 {FirstTime:yyyy-MM-dd HH:mm:ss}，你的2022年的祈愿记录数据可能不完整。";
            }
            else
            {
                return $"第一条记录的抽取时间是 {FirstTime:yyyy-MM-dd HH:mm:ss}，你的2022年的祈愿记录数据应该是完整的。";
            }
        }
    }

    public int Count_2022 { get; set; }

    public int Cost => Count_2022 * 160;

    /// <summary>
    /// 单抽
    /// </summary>
    public int Count_dc { get; set; }

    /// <summary>
    /// 十连
    /// </summary>
    public int Count_sl { get; set; }

    /// <summary>
    /// 十连多金
    /// </summary>
    public List<Summary2022_WishlogReport_Sldj_十连多金> List_Sldj { get; set; }

    public string Str_sldj
    {
        get
        {
            if (List_Sldj is null or { Count: 0 })
            {
                return "很遗憾，没有出现十连多金的情况。";
            }
            else
            {
                return $"很幸运，出现了{string.Join('，', List_Sldj.Select(x => $"{x.Count}次十连{x.Rank5}金"))}";
            }
        }
    }

    public int Count_Rank5 { get; set; }

    public int Count_Rank4 { get; set; }

    /// <summary>
    /// 常驻
    /// </summary>
    public int Count_200 { get; set; }

    /// <summary>
    /// 角色
    /// </summary>
    public int Count_301 { get; set; }

    /// <summary>
    /// 武器
    /// </summary>
    public int Count_302 { get; set; }

    /// <summary>
    /// 限定
    /// </summary>
    public int Count_xd { get; set; }

    /// <summary>
    /// 限定歪
    /// </summary>
    public int Count_xd_noup { get; set; }

    /// <summary>
    /// 最欧
    /// </summary>
    public int GuaranteeIndex_zo { get; set; }

    /// <summary>
    /// 最非
    /// </summary>
    public int GuaranteeIndex_zf { get; set; }

    /// <summary>
    /// 最欧
    /// </summary>
    public List<WishlogItemEx> List_zo { get; set; }

    /// <summary>
    /// 最非
    /// </summary>
    public List<WishlogItemEx> List_zf { get; set; }


    public string Str_zo => string.Join(" ", List_zo?.Select(x => $"{x.Name}[{GuaranteeIndex_zo}]") ?? new List<string> { "还没有抽到五星卡" });

    public string Str_zf => string.Join(" ", List_zf?.Select(x => $"{x.Name}[{GuaranteeIndex_zf}]") ?? new List<string> { "还没有抽到五星卡" });


    public int TotalCount_WishEvent { get; set; }

    public int GetCount_WishEvent { get; set; }

    public Summary2022_WishlogReport_WishEvent? WishEvent_MaxCount { get; set; }

    public int Count_Character_2022_Total { get; set; }

    public int Count_Character_2022_New { get; set; }

    public Summary2022_WishlogReport_MaxByName? Max_byname_character { get; set; }

    public Summary2022_WishlogReport_MaxByName? Max_byname_weapon { get; set; }

}


public class Summary2022_WishlogReport_Sldj_十连多金
{
    public int Rank5 { get; set; }

    public int Count { get; set; }

}


public class Summary2022_WishlogReport_WishEvent
{
    public WishEventInfo WishEvent { get; set; }

    public int Count { get; set; }

    public int Count_Rank_5 { get; set; }

    public int Count_Rank_4 { get; set; }
}


public class Summary2022_WishlogReport_MaxByName
{
    public string Name { get; set; }

    public int Count { get; set; }

    public string Message => $"{Name}({Count})";
}