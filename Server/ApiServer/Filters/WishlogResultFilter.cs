using Microsoft.AspNetCore.Mvc.Filters;

namespace Xunkong.ApiServer.Filters;

public class WishlogRecordFilter : IAsyncResultFilter
{

    private readonly ILogger<WishlogRecordFilter> _logger;

    private readonly XunkongDbContext _dbContext;

    public WishlogRecordFilter(ILogger<WishlogRecordFilter> logger, XunkongDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }


    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        var record = FilterHelper.GetRecordModel<WishlogRecordModel>(context);
        record.ReturnCode = 0;
        record.Message = "OK";
        var operation = record.Path?.Substring(record.Path.LastIndexOf('/') + 1).ToLower();
        if (context.Result is ObjectResult obj)
        {
            if ((obj.Value as dynamic)?.Data is WishlogBackupResult result)
            {
                record.Uid = result.Uid;
                record.Operation = operation;
                record.CurrentCount = result.CurrentCount;
                record.GetCount = result.GetCount;
                record.PutCount = result.PutCount;
                record.DeleteCount = result.DeleteCount;
            }
        }
        try
        {
            _dbContext.WishlogRecords.Add(record);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in save record of wishlog result filter.");
        }
        await next();
    }

}

