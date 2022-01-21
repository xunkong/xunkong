using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Xunkong.Core.XunkongApi;
using Xunkong.Web.Api.Services;

namespace Xunkong.Web.Api.Controllers
{
    [ApiController]
    [ApiVersion("0")]
    [Route("v{version:ApiVersion}/[controller]")]
    public class MetadataController : ControllerBase
    {
        private readonly ILogger<MetadataController> _logger;
        private readonly XunkongDbContext _dbContext;
        private readonly IMemoryCache _cache;

        public MetadataController(ILogger<MetadataController> logger, XunkongDbContext dbContext, IMemoryCache cache)
        {
            _logger = logger;
            _dbContext = dbContext;
            _cache = cache;
        }


        [HttpGet("character")]
        public async Task<ResponseDto> GetCharacterInfos()
        {
            if (_cache.TryGetValue("characterinfos", out ResponseDto result))
            {
                return result;
            }
            var list = await _dbContext.CharacterInfos.AsNoTracking().Where(x => x.ConstllationName != null).ToListAsync();
            result = ResponseDto.Ok(new { Count = list.Count, List = list });
            _cache.Set("characterinfos", result);
            return result;
        }


        [HttpGet("weapon")]
        public async Task<ResponseDto> GetWeaponInfos()
        {
            if (_cache.TryGetValue("weaponinfos", out ResponseDto result))
            {
                return result;
            }
            var list = await _dbContext.WeaponInfos.AsNoTracking().Where(x => x.GachaIcon != null).ToListAsync();
            result = ResponseDto.Ok(new { Count = list.Count, List = list });
            _cache.Set("weaponinfos", result);
            return result;
        }


        [HttpGet("wishevent")]
        public async Task<ResponseDto> GetWishEventInfos()
        {
            if (_cache.TryGetValue("wisheventinfos", out ResponseDto result))
            {
                return result;
            }
            var list = await _dbContext.WishEventInfos.AsNoTracking().ToListAsync();
            result = ResponseDto.Ok(new { Count = list.Count, List = list });
            _cache.Set("wisheventinfos", result);
            return result;
        }

    }
}
