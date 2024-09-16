namespace Xunkong.Hoyolab.Wishlog;

struct QueryParam
{
    public WishType WishType { get; set; }

    public int Page { get; set; }

    public long EndId { get; set; }

    public int Size { get; set; }


    /// <summary>
    /// 获取一页祈愿记录时的查询参数
    /// </summary>
    /// <param name="type"></param>
    /// <param name="page"></param>
    /// <param name="size">这一页有多少条记录，不超过20，默认6</param>
    /// <param name="endId">上一页记录中最后一条的id，为0则从最新的记录开始获取</param>
    public QueryParam(WishType type, int page, int size = 6, long endId = 0)
    {
        WishType = type;
        Page = page;
        Size = size;
        EndId = endId;
    }

    public override string ToString()
    {
        return $"gacha_type={(int)WishType}&page={Page}&size={Size}&end_id={EndId}";
    }
}
