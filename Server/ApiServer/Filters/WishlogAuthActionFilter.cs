using Microsoft.AspNetCore.Mvc.Filters;
using Xunkong.Hoyolab;

namespace Xunkong.ApiServer.Filters;

internal class WishlogAuthActionFilter : IAsyncActionFilter
{

    private readonly ILogger<WishlogAuthActionFilter> _logger;

    private readonly XunkongDbContext _dbContext;

    private readonly WishlogClient _wishlogClient;

    public WishlogAuthActionFilter(ILogger<WishlogAuthActionFilter> logger, XunkongDbContext dbContext, WishlogClient wishlogClient)
    {
        _logger = logger;
        _dbContext = dbContext;
        _wishlogClient = wishlogClient;
    }


    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ActionArguments["wishlog"] is WishlogBackupRequest wishlog)
        {
            if (string.IsNullOrWhiteSpace(wishlog.Url))
            {
                throw new XunkongApiServerException(ErrorCode.UrlFormatError);
            }
            try
            {
                if (!await CheckUidAndUrlAsync(wishlog.Uid, wishlog.Url))
                {
                    throw new XunkongApiServerException(ErrorCode.UrlNotMatchUid);
                }
            }
            catch (HoyolabException ex)
            {
                throw new XunkongApiServerException(ErrorCode.HoyolabException, ex.Message);
            }
            catch (ArgumentException ex)
            {
                throw new XunkongApiServerException(ErrorCode.UrlFormatError, ex.Message);
            }
            await next();
        }
        else
        {
            throw new XunkongApiServerException(ErrorCode.InvalidModelException);
        }
    }



    private async Task<bool> CheckUidAndUrlAsync(int uid, string url)
    {
        var key = await _dbContext.WishlogAuthkeys.AsNoTracking().FirstOrDefaultAsync(x => x.Uid == uid);
        if (key is not null)
        {
            if (DateTime.UtcNow < key.DateTime + new TimeSpan(24, 0, 0) && key.Url == url)
            {
                return true;
            }
        }
        var newUid = await _wishlogClient.GetUidAsync(url);
        var info = new WishlogAuthkeyItem
        {
            Url = url,
            Uid = newUid,
            DateTime = DateTime.UtcNow,
        };
        if (_dbContext.WishlogAuthkeys.Any(x => x.Uid == info.Uid))
        {
            _dbContext.Update(info);
        }
        else
        {
            _dbContext.Add(info);
        }
        await _dbContext.SaveChangesAsync();
        return uid == newUid;
    }


}
