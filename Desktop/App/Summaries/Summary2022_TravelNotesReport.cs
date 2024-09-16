namespace Xunkong.Desktop.Summaries;

public class Summary2022_TravelNotesReport
{
    public int Uid { get; set; }

    public bool IsFirstTimeIn2022 { get; set; }

    public DateTime FirstTime { get; set; }

    public string Str_FirstTime
    {
        get
        {
            if (IsFirstTimeIn2022)
            {
                return $"第一条记录的获取时间是 {FirstTime:yyyy-MM-dd HH:mm:ss}，你的2022年的旅行札记数据可能不完整。";
            }
            else
            {
                return $"第一条记录的获取时间是 {FirstTime:yyyy-MM-dd HH:mm:ss}，你的2022年的旅行札记数据应该是完整的。";
            }
        }
    }

    public int Count_p { get; set; }

    public int Count_m { get; set; }

    public int Sum_p { get; set; }

    public int Sum_m { get; set; }

    public int Count_date { get; set; }

    /// <summary>
    /// 成就奖励
    /// </summary>
    public int Count_cj { get; set; }

    /// <summary>
    /// 每日委托
    /// </summary>
    public int Count_mrwt { get; set; }

    /// <summary>
    /// 突发事件
    /// </summary>
    public int Count_tfsj { get; set; }

    /// <summary>
    /// 凌晨
    /// </summary>
    public int Count_date_lc { get; set; }

    public DateTime Time_lc { get; set; }

    public string Str_lc
    {
        get
        {
            if (Count_date_lc == 0)
            {
                return "你的每日任务绝不会拖到第二天";
            }
            else
            {
                return $"最晚的一次是 {Time_lc:yyyy-MM-dd HH:mm:ss}";
            }
        }
    }

    public Summary2022_TravelNotesReport_SumByMonth Max_month_p { get; set; }

    public Summary2022_TravelNotesReport_SumByMonth Max_month_m { get; set; }

    public Summary2022_TravelNotesReport_SumByDay Max_day_p { get; set; }

    public Summary2022_TravelNotesReport_SumByDay Max_day_m { get; set; }

    /// <summary>
    /// 击杀怪物
    /// </summary>
    public Summary2022_TravelNotesReport_SumByDay Max_jsgw { get; set; }

    public List<Summary2022_TravelNotesReport_SumByAction> List_action_p { get; set; }

    public List<Summary2022_TravelNotesReport_SumByAction> List_action_m { get; set; }
}


public class Summary2022_TravelNotesReport_SumByMonth
{
    public int Month { get; set; }

    public int Count { get; set; }

    public int Sum { get; set; }
}


public class Summary2022_TravelNotesReport_SumByDay
{
    public DateTime Date { get; set; }

    public int Count { get; set; }

    public int Sum { get; set; }

    public string DateAndSumString => $"{Date:yyyy-MM-dd} ({Sum})";

    public string DateString => $"{Date:yyyy-MM-dd}";
}


public class Summary2022_TravelNotesReport_SumByAction
{
    public string ActionName { get; set; }

    public int Count { get; set; }

    public int Sum { get; set; }
}