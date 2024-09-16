using Microsoft.AspNetCore.Mvc.Filters;

namespace Xunkong.ApiServer.Filters;

public class ExceptionFilter : IAsyncExceptionFilter
{

    private readonly ILogger<ExceptionFilter> _logger;

    private readonly XunkongDbContext _dbContext;


    public ExceptionFilter(ILogger<ExceptionFilter> logger, XunkongDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }


    public async Task OnExceptionAsync(ExceptionContext context)
    {
        var record = FilterHelper.GetRecordModel<BaseRecordModel>(context);
        if (context.Exception is XunkongApiServerException ex)
        {
            record.ReturnCode = ex.Code;
            record.Message = ex.Message;
            _logger.LogInformation(context.Exception, "XunkongApiServerException");
        }
        else
        {
            record.ReturnCode = ErrorCode.InternalException;
            record.Message = context.Exception.Message;
            _logger.LogError(context.Exception, "Unknown exception in controller.");
        }
        try
        {
            _dbContext.AllRecords.Add(record);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in save record of exception filter.");
        }
        context.Result = new JsonResult(new { Code = record.ReturnCode, record.Message, Data = null as object });
    }
}
