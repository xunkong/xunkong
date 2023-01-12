using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xunkong.Hoyolab.Wishlog;

namespace Xunkong.Desktop.Models;



public class WishlogSummaryPage_ItemThumb
{
    public WishlogSummaryPage_ItemThumb(string? name, int rarity, ElementType element, WeaponType weaponType, int count, string? icon, DateTimeOffset lastTime)
    {
        Name = name;
        Rarity = rarity;
        Element = element;
        WeaponType = weaponType;
        Count = count;
        Icon = icon;
        LastTime = lastTime;
    }

    public string? Name { get; set; }

    public int Rarity { get; set; }

    public ElementType Element { get; set; }

    public WeaponType WeaponType { get; set; }

    public int Count { get; set; }

    public string? Icon { get; set; }

    public DateTimeOffset LastTime { get; set; }
}



public class WishlogSummaryPage_QueryTypeStats
{
    public WishlogSummaryPage_QueryTypeStats(WishType queryType,
                                             int totalCount,
                                             int rank5Count,
                                             int rank4Count,
                                             DateTimeOffset startTime,
                                             DateTimeOffset endTime,
                                             int upItemCount,
                                             int currentGuarantee,
                                             int maxGuarantee,
                                             int minGuarantee,
                                             List<WishlogSummaryPage_Rank5Item> rank5Items,
                                             bool lastRank5ItemIsUp,
                                             int lastRank5ItemGuarantee)
    {
        QueryType = queryType;
        TotalCount = totalCount;
        Rank5Count = rank5Count;
        Rank4Count = rank4Count;
        StartTime = startTime;
        EndTime = endTime;
        UpItemCount = upItemCount;
        CurrentGuarantee = currentGuarantee;
        MaxGuarantee = maxGuarantee;
        MinGuarantee = minGuarantee;
        Rank5Items = rank5Items;
        LastRank5ItemIsUp = lastRank5ItemIsUp;
        LastRank5ItemGuarantee = lastRank5ItemGuarantee;
    }

    public WishType QueryType { get; set; }

    public int TotalCount { get; set; }

    public int Rank5Count { get; set; }

    public int Rank4Count { get; set; }

    public DateTimeOffset StartTime { get; set; }

    public DateTimeOffset EndTime { get; set; }

    public int UpItemCount { get; set; }

    public int CurrentGuarantee { get; set; }

    public int MaxGuarantee { get; set; }

    public int MinGuarantee { get; set; }

    public List<WishlogSummaryPage_Rank5Item> Rank5Items { get; set; }

    public bool LastRank5ItemIsUp { get; set; }

    public int LastRank5ItemGuarantee { get; set; }


    public bool AnyData => TotalCount > 0;

    public int Rank3Count => TotalCount - Rank5Count - Rank4Count;

    public string Rank5Ratio => ((double)Rank5Count / TotalCount).ToString("P3");

    public string Rank4Ratio => ((double)Rank4Count / TotalCount).ToString("P3");

    public string Rank3Ratio => ((double)Rank3Count / TotalCount).ToString("P3");

    public string GuaranteeType => QueryType == WishType.Permanent ? "保底内" : LastRank5ItemIsUp ? "小保底内" : "大保底内";

    public int BaodiWai => QueryType == WishType.Permanent ? 0 : (Rank5Count - UpItemCount);

    public int BaodiBuwai => QueryType == WishType.Permanent ? Rank5Count : Rank5Count == 0 ? 0 : (2 * UpItemCount - Rank5Count + (LastRank5ItemIsUp ? 0 : 1));

    public string AverageRank5Count => ((TotalCount - CurrentGuarantee) / (double)Rank5Count).ToString("F2");

    public string AverageUpRank5Count => QueryType == WishType.Permanent ? "0" : ((TotalCount - CurrentGuarantee - (LastRank5ItemIsUp ? 0 : LastRank5ItemGuarantee)) / (double)UpItemCount).ToString("F2");

}



public class WishlogSummaryPage_Rank5Item : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public string Name { get; set; }

    public int Guarantee { get; set; }

    public DateTimeOffset Time { get; set; }

    public bool IsUp { get; set; }

    public string Color { get; set; }


    private string _Foreground;

    public WishlogSummaryPage_Rank5Item(string name, int guarantee, DateTimeOffset time, bool isUp)
    {
        Name = name;
        Guarantee = guarantee;
        Time = time;
        IsUp = isUp;
    }

    public string Foreground
    {
        get { return _Foreground; }
        set
        {
            if (_Foreground != value)
            {
                _Foreground = value;
                OnPropertyChanged();
            }
        }
    }

}



public class WishlogSummaryPage_EventStats
{

    public string Name { get; set; }


    public string Version { get; set; }


    public DateTimeOffset StartTime { get; set; }


    public DateTimeOffset EndTime { get; set; }


    public int TotalCount { get; set; }


    public int Rank5Count { get; set; }


    public int Rank4Count { get; set; }


    public int Rank3Count { get; set; }


    public List<WishlogSummaryPage_UpItem> UpItems { get; set; }


    public List<WishlogSummaryPage_Rank5Item> Rank5Items { get; set; }

}



public class WishlogSummaryPage_UpItem
{

    public string Name { get; set; }


    public int Rarity { get; set; }


    public string Icon { get; set; }


    public ElementType Element { get; set; }


    public WeaponType WeaponType { get; set; }


    public int Count { get; set; }


    public bool AnyData => Count > 0;

}
