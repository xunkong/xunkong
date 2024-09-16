using Xunkong.Hoyolab.Wishlog;

namespace Xunkong.ApiClient;

public class WishlogBackupResult
{

    public int Uid { get; set; }


    public int CurrentCount { get; set; }


    public int GetCount { get; set; }


    public int PutCount { get; set; }


    public int DeleteCount { get; set; }


    public List<WishlogItem>? List { get; set; }


    public WishlogBackupResult() { }

    public WishlogBackupResult(int uid, int currentCount, int getCount, int putCount, int deleteCount, List<WishlogItem>? list = null)
    {
        Uid = uid;
        CurrentCount = currentCount;
        GetCount = getCount;
        PutCount = putCount;
        DeleteCount = deleteCount;
        List = list;
    }


}
