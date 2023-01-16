using Xunkong.Hoyolab.Wishlog;

namespace Xunkong.Desktop.Models;

public class WishlogItemEx : WishlogItem
{

    public int Index { get; set; }


    public int GuaranteeIndex { get; set; }


    public string Version { get; set; }


    public string WishEventName { get; set; }


    public string VersionAndName => string.IsNullOrWhiteSpace(Version) ? WishEventName : $"{Version} {WishEventName}";


    public bool IsUp { get; set; }


    public bool IsDabaodi { get; set; }


    public string GuaranteeType => (QueryType, IsUp, IsDabaodi) switch
    {
        (WishType.Novice, _, _) => "新手池",
        (WishType.Permanent, _, _) => "常驻池",
        (_, true, false) => "小保底",
        (_, true, true) => "大保底",
        (_, false, _) => "歪了",
    };


}
