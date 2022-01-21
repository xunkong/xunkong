using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Xunkong.Core.Hoyolab;
using Xunkong.Core.Wish;
using Xunkong.Core.XunkongApi;
using Xunkong.Web.Api.Models;
using Xunkong.Web.Api.Services;

namespace Xunkong.Web.Api.Filters
{
    public class WishlogAuthActionFilter : IAsyncActionFilter
    {
        private readonly ILogger<WishlogAuthActionFilter> _logger;

        private readonly XunkongDbContext _dbContext;


        public WishlogAuthActionFilter(ILogger<WishlogAuthActionFilter> logger, XunkongDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }


        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.ActionArguments["wishlog"] is WishlogDto wishlog)
            {
                var uid = wishlog.Uid;
                if (wishlog.Url is null)
                {
                    throw new XunkongServerException(ReturnCode.UrlFormatError);
                }
                try
                {
                    var url_uid = await GetUidByUrlAsync(wishlog.Url);
                    if (url_uid != uid)
                    {
                        throw new XunkongServerException(ReturnCode.UrlNotMatchUid);
                    }
                }
                catch (HoyolabException ex)
                {
                    throw new XunkongServerException(ReturnCode.HoyolabException, ex.Message);
                }
                catch (ArgumentException ex)
                {
                    throw new XunkongServerException(ReturnCode.UrlFormatError, ex.Message);
                }
                await next();
            }
            else
            {
                throw new XunkongServerException(ReturnCode.InvalidModelException);
            }
        }



        private async Task<int> GetUidByUrlAsync(string url)
        {
            var key = await _dbContext.WishlogAuthkeys.AsNoTracking().FirstOrDefaultAsync(x => x.Url == url);
            if (key is not null)
            {
                if (DateTime.UtcNow < key.DateTime + new TimeSpan(24, 0, 0))
                {
                    return key.Uid;
                }
            }
            var uid = await new WishlogClient(url).GetUidAsync();
            var info = new WishlogAuthkeyItem
            {
                Url = url,
                Uid = uid,
                DateTime = DateTime.UtcNow,
            };
            if (_dbContext.WishlogAuthkeys.Any(x => x.Url == info.Url))
            {
                _dbContext.Update(info);
            }
            else
            {
                _dbContext.Add(info);
            }
            await _dbContext.SaveChangesAsync();
            return uid;
        }


    }
}
