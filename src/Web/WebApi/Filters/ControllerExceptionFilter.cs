using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Xunkong.Core.XunkongApi;
using Xunkong.Web.Api.Models;
using Xunkong.Web.Api.Services;

namespace Xunkong.Web.Api.Filters
{
    public class ControllerExceptionFilter : IAsyncExceptionFilter
    {

        private readonly ILogger<ControllerExceptionFilter> _logger;

        private readonly XunkongDbContext _dbContext;


        public ControllerExceptionFilter(ILogger<ControllerExceptionFilter> logger, XunkongDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }


        public async Task OnExceptionAsync(ExceptionContext context)
        {
            var requestId = context.HttpContext.Request.Headers["X-Fc-Request-Id"];
            var method = context.HttpContext.Request.Method;
            var path = context.HttpContext.Request.Path.Value;
            var ua = context.HttpContext.Request.Headers["User-Agent"];
            var ip = context.HttpContext.Request.Headers["X-Forwarded-For"];
            var deviceId = context.HttpContext.Request.Headers["X-Device-Id"];
            var platform = context.HttpContext.Request.Headers["X-Platform"];
            var channel = context.HttpContext.Request.Headers["X-Channel"];
            var version = context.HttpContext.Request.Headers["X-Version"];
            var record = new BaseRecordModel
            {
                RequestId = string.IsNullOrWhiteSpace(requestId) ? Guid.NewGuid().ToString("D") : requestId,
                DateTime = DateTimeOffset.UtcNow,
                Path = path,
                Method = method,
                StatusCode = 200,
                DeviceId = deviceId,
                UserAgent = ua,
                Ip = ip,
                Platform = platform,
                Channel = channel,
                Version = version,
            };
            if (context.Exception is XunkongException ex)
            {
                record.ReturnCode = ex.Code;
                record.Message = ex.Message;
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
            context.Result = new JsonResult(new ResponseBaseWrapper((ErrorCode)record.ReturnCode, record.Message));
        }
    }
}
