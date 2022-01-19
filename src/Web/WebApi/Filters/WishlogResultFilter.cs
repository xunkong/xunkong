using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Xunkong.Core.XunkongApi;
using Xunkong.Web.Api.Models;
using Xunkong.Web.Api.Services;

namespace Xunkong.Web.Api.Filters
{
    public class WishlogResultFilter : IAsyncResultFilter
    {


        private readonly ILogger<WishlogResultFilter> _logger;
        private readonly XunkongDbContext _dbContext;

        public WishlogResultFilter(ILogger<WishlogResultFilter> logger, XunkongDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }


        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var requestId = context.HttpContext.Request.Headers["X-Fc-Request-Id"];
            var method = context.HttpContext.Request.Method;
            var path = context.HttpContext.Request.Path.Value;
            var operation = path?.Substring(path.LastIndexOf('/') + 1).ToLower();
            var ua = context.HttpContext.Request.Headers["User-Agent"];
            var ip = context.HttpContext.Request.Headers["X-Forwarded-For"];
            var deviceId = context.HttpContext.Request.Headers["X-Device-Id"];
            var record = new WishlogRecordModel
            {
                RequestId = string.IsNullOrWhiteSpace(requestId) ? Guid.NewGuid().ToString("D") : requestId,
                DateTime = DateTimeOffset.UtcNow,
                Path = path,
                Method = method,
                StatusCode = 200,
                DeviceId = deviceId,
                UserAgent = ua,
                Ip = ip,
            };
            if (context.Result is ObjectResult j)
            {
                if (j.Value is ResponseData data)
                {
                    record.ReturnCode = data.Code;
                    record.Message = data.Message;
                    if (data.Data is WishlogResult result)
                    {
                        record.Uid = result.Uid;
                        record.Operation = operation;
                        record.CurrentCount = result.CurrentCount;
                        record.GetCount = result.GetCount;
                        record.PutCount = result.PutCount;
                        record.DeleteCount = result.DeleteCount;
                    }
                };
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
}

