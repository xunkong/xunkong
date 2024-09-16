using Microsoft.AspNetCore.Mvc.Filters;

namespace Xunkong.ApiServer.Filters;

public class BaseRecordResultFilter : IAsyncResultFilter
{

    private readonly ILogger<WishlogRecordFilter> _logger;

    private readonly XunkongDbContext _dbContext;

    public BaseRecordResultFilter(ILogger<WishlogRecordFilter> logger, XunkongDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }


    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        var record = FilterHelper.GetRecordModel<BaseRecordModel>(context);
        record.ReturnCode = 0;
        record.Message = "OK";
        try
        {
            _dbContext.AllRecords.Add(record);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in save record of base record filter.");
        }
        await next();
    }


}
