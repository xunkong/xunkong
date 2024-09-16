// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Xunkong.ApiServer.Controllers;

/// <summary>
/// 祈愿记录托管
/// </summary>
[ApiController]
[ApiVersion("1")]
[Route("v{version:ApiVersion}/[controller]")]
//[ServiceFilter(typeof(WishlogRecordFilter))]
[ServiceFilter(typeof(WishlogAuthActionFilter))]
[ResponseCache(NoStore = true)]
public class WishlogController : ControllerBase
{
    private readonly ILogger<WishlogController> _logger;


    private readonly XunkongDbContext _dbContext;



    public WishlogController(ILogger<WishlogController> logger, XunkongDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }


    /// <summary>
    /// 获取数据，大于 EndId
    /// </summary>
    /// <param name="wishlog"></param>
    /// <returns></returns>
    [HttpPost("get")]
    public async Task<WishlogBackupResult> GetWishlogAsync([FromBody] WishlogBackupRequest wishlog)
    {
        var uid = wishlog.Uid;
        var currentCount = await _dbContext.WishlogItems.Where(x => x.Uid == uid).CountAsync();
        if (currentCount == 0)
        {
            _logger.LogInformation("Wishlog:\nUid: {Uid}\nMethod: Get\nCount: 0", uid);
            return new WishlogBackupResult(uid, 0, 0, 0, 0);
        }
        var list = await _dbContext.WishlogItems.AsNoTracking()
                                                .Where(x => x.Uid == uid && x.Id > wishlog.LastId)
                                                .OrderBy(x => x.Id)
                                                .ToListAsync();
        _logger.LogInformation("Wishlog:\nUid: {Uid}\nMethod: Get\nCount: {Count}", uid, list.Count);
        return new WishlogBackupResult(uid, currentCount, list.Count, 0, 0, list);
    }


    /// <summary>
    /// 上传数据
    /// </summary>
    /// <param name="wishlog"></param>
    /// <returns></returns>
    [HttpPost("put")]
    public async Task<WishlogBackupResult> UpdateWishlogAsync([FromBody] WishlogBackupRequest wishlog)
    {
        var uid = wishlog.Uid;
        var list = wishlog.List?.Where(x => x.Uid == uid).ToList();
        if (list is null || !list.Any())
        {
            _logger.LogInformation("Wishlog:\nUid: {Uid}\nMethod: Put\nError: Put item is 0", uid);
            throw new XunkongApiServerException(ErrorCode.NoWishlogItem);
        }
        var existing = await _dbContext.WishlogItems.Where(x => x.Uid == uid).Select(x => x.Id).ToListAsync();
        var inserting = list.ExceptBy(existing, x => x.Id).ToList();
        foreach (var item in inserting)
        {
            item.QueryType = item.WishType switch
            {
                WishType.CharacterEvent_2 => WishType.CharacterEvent,
                _ => item.WishType,
            };
        }
        _dbContext.AddRange(inserting);
        var putCount = await _dbContext.SaveChangesAsync();
        var currentCount = await _dbContext.WishlogItems.Where(x => x.Uid == uid).CountAsync();
        _logger.LogInformation("Wishlog:\nUid: {Uid}\nMethod: Put\nPut: {Put}\nExisted: {Existed}\nInserting: {Inserting}\nTotal: {Total}", uid, list.Count, existing.Count, inserting.Count, currentCount);
        return new WishlogBackupResult(uid, currentCount, 0, putCount, 0, null);
    }


    /// <summary>
    /// 删除指定用户的所有数据
    /// </summary>
    /// <param name="wishlog"></param>
    /// <returns></returns>
    [HttpPost("delete")]
    public async Task<WishlogBackupResult> DeleteWishlogAsync([FromBody] WishlogBackupRequest wishlog)
    {
        var uid = wishlog.Uid;
        using var t = await _dbContext.Database.BeginTransactionAsync();
        int deleteCount = 0;
        try
        {
            _logger.LogInformation("Wishlog:\nUid: {Uid}\nMethod: Delete", uid);
            deleteCount = await _dbContext.Database.ExecuteSqlRawAsync($"DELETE FROM wishlog_items WHERE Uid={uid};");
            await t.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Wishlog:\nUid: {Uid}\nMethod: Delete\nError: {Error}", uid, ex.Message);
            await t.RollbackAsync();
            throw;
        }
        _logger.LogInformation("Wishlog:\nUid: {Uid}\nMethod: Delete\nCount: {Count}", uid, deleteCount);
        return new WishlogBackupResult(uid, 0, 0, 0, deleteCount, null);
    }



    /// <summary>
    /// 获取指定用户的祈愿统计和最后一条数据
    /// </summary>
    /// <param name="wishlog"></param>
    /// <returns></returns>
    [HttpPost("last")]
    public async Task<WishlogBackupResult> GetLastWishlogAsync([FromBody] WishlogBackupRequest wishlog)
    {
        var uid = wishlog.Uid;
        var currentCount = await _dbContext.WishlogItems.Where(x => x.Uid == uid).CountAsync();
        if (currentCount == 0)
        {
            _logger.LogInformation("Wishlog:\nUid: {Uid}\nMethod: Last\nCount: 0\nLastId: 0", uid);
            return new WishlogBackupResult(uid, 0, 0, 0, 0);
        }
        var item = await _dbContext.WishlogItems.AsNoTracking().Where(x => x.Uid == uid).OrderBy(x => x.Id).LastOrDefaultAsync();
        _logger.LogInformation("Wishlog:\nUid: {Uid}\nMethod: Last\nCount: {Count}\nLastId: {LastId}", uid, currentCount, item.Id);
        return new WishlogBackupResult(uid, currentCount, 1, 0, 0, new List<WishlogItem> { item! });
    }




}
