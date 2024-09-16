using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;
using Xunkong.ApiClient.Xunkong;

namespace Xunkong.ApiServer.Controllers;

[ApiController]
[ApiVersion("1")]
[Route("v{version:ApiVersion}/[controller]")]
//[ServiceFilter(typeof(BaseRecordResultFilter))]
public class WallpaperController : Controller
{

    public interface IWallpaperList
    {
        public List<WallpaperInfo> List { get; init; }
    }

    public record WallpaperListResult(int Count, List<WallpaperInfo> List) : IWallpaperList;

    public record WallpaperSearchResult(int Total, int Offset, int Take, List<string> Keys, List<WallpaperInfo> List) : IWallpaperList;



    private readonly ILogger<WallpaperController> _logger;

    private readonly XunkongDbContext _dbContext;

    private readonly DbConnectionFactory _factory;


    public WallpaperController(ILogger<WallpaperController> logger, XunkongDbContext dbContext, DbConnectionFactory factory)
    {
        _logger = logger;
        _dbContext = dbContext;
        _factory = factory;
    }




    [HttpGet("{id}")]
    [ResponseCache(Duration = 2592000)]
    public async Task<WallpaperInfo> GetWallpaperInfoByIdAsync([FromRoute] int id)
    {
        var info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Id == id).SingleOrDefaultAsync();
        if (info is not null)
        {
            return info;
        }
        else
        {
            throw new XunkongApiServerException(ErrorCode.InternalException, "Wallpaper not found.");
        }
    }



    [NoWrapper]
    [HttpGet("{id}/redirect")]
    [ResponseCache(Duration = 2592000)]
    public async Task<IActionResult> RedirectToWallpaperImageByIdAsync([FromRoute] int id)
    {
        var info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => x.Id == id).SingleOrDefaultAsync();
        if (info is not null)
        {
            return Redirect(info.Url);
        }
        else
        {
            return NotFound("Wallpaper not found.");
        }
    }



    [HttpGet("recommend")]
    [ResponseCache(Duration = 3600)]
    public async Task<WallpaperInfo> GetRecommendWallpaperInfoAsync()
    {
        return await RandomNextAsync();
    }


    /// <summary>
    /// 不要删除
    /// </summary>
    /// <returns></returns>
    [NoWrapper]
    [HttpGet("recommend/redirect")]
    [ResponseCache(Duration = 3600)]
    public async Task<IActionResult> RedirectToRecommendWallpaperImageAsync()
    {
        var info = await RandomNextAsync();
        return Redirect(info.Url);
    }



    [HttpGet("random")]
    [ResponseCache(Duration = 5)]
    public async Task<WallpaperInfo> GetRandomWallpaperInfoAsync()
    {
        var info = await RandomNextAsync();
        _logger.LogInformation("Wallpaper:\nId: {Id}\nTitle: {Title}\nUrl: {Url}", info.Id, info.Title, info.Url);
        return info;
    }


    /// <summary>
    /// 不要删除
    /// </summary>
    /// <returns></returns>
    [NoWrapper]
    [HttpGet("random/redirect")]
    [ResponseCache(Duration = 5)]
    public async Task<IActionResult> RedirectToRandomWallpaperImageAsync()
    {
        var info = await RandomNextAsync();
        return Redirect(info.Url);
    }



    [HttpGet("next")]
    [ResponseCache(Duration = 604800)]
    public async Task<WallpaperInfo> GetNextWallpaperInfoAsync([FromQuery] int lastId)
    {
        return await RandomNextAsync();
    }



    [HttpGet("list")]
    [ResponseCache(Duration = 5)]
    public async Task<WallpaperListResult> GetWallpaperInfosAsync([FromQuery] int size = 20)
    {
        size = Math.Clamp(size, 10, 100);
        var list = new List<WallpaperInfo>(size);
        for (int i = 0; i < size; i++)
        {
            list.Add(await RandomNextAsync());
        }
        return new WallpaperListResult(size, list);
    }



    [HttpPost("rating")]
    [ResponseCache(NoStore = true)]
    public async Task RatingWallpaperAsync([FromHeader(Name = "X-Device-Id")] string deviceId, [FromBody] List<WallpaperRating> ratings)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            throw new XunkongApiException(-1, "DeviceId is wrong.");
        }
        foreach (var rating in ratings)
        {
            if (rating.DeviceId != deviceId)
            {
                throw new XunkongApiException(-1, "DeviceId is wrong.");
            }
            rating.Rating = Math.Clamp(rating.Rating, -1, 5);
        }
        using var dapper = _factory.CreateDbConnection();
        await dapper.OpenAsync();
        using var t = await dapper.BeginTransactionAsync();
        await dapper.ExecuteAsync("""
            INSERT INTO wallpaper_rating (WallpaperId, DeviceId, Rating, Time)
            VALUES (@WallpaperId, @DeviceId, @Rating, @Time)
            ON DUPLICATE KEY UPDATE Rating=@Rating, Time=@Time;
            """, ratings, t);
        await t.CommitAsync();
    }


    [HttpGet("search")]
    [ResponseCache(Duration = 604800)]
    public async Task<WallpaperSearchResult> SearchWallpaperAsync([FromQuery(Name = "key")] string[] keys, [FromQuery] int offset = 0, [FromQuery] int take = 20)
    {
        var words = keys.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        var regex = string.Join("|", words.Select(Regex.Escape));
        using var dapper = _factory.CreateDbConnection();
        var total = await dapper.QueryFirstOrDefaultAsync<int>("SELECT COUNT(*) FROM wallpapers WHERE CONCAT_WS(' ', Title, Author, Description, Tags) REGEXP @regex;", new { regex });
        take = Math.Clamp(take, 1, 100);
        offset = Math.Clamp(offset, 0, Math.Clamp(total - take, 0, int.MaxValue));
        var sb = new StringBuilder();
        sb.Append("SELECT *, (");
        for (int i = 1; i < words.Count + 1; i++)
        {
            sb.Append($"IF(CONCAT_WS(' ', Title, Author, Description, Tags) LIKE {{{i}}}, 1, 0) + ");
        }
        sb.Append($"0) AS weight FROM wallpapers WHERE CONCAT_WS(' ', Title, Author, Description, Tags) REGEXP {{0}} ORDER BY weight DESC, Rating DESC LIMIT {offset},{take};");
        var param = new List<string>(words.Count + 1) { regex };
        param.AddRange(words.Select(x => $"%{x}%"));
        var list = await _dbContext.WallpaperInfos.FromSqlRaw(sb.ToString(), param.ToArray()).ToListAsync();
        return new WallpaperSearchResult(total, offset, take, words, list);
    }



    [HttpPost("getInfosByIds")]
    public async Task<WallpaperListResult> GetInfosByIdsAsync([FromBody] List<int> ids)
    {
        var info = await _dbContext.WallpaperInfos.AsNoTracking().Where(x => ids.Contains(x.Id)).ToListAsync();
        return new WallpaperListResult(info.Count, info);
    }




    private static ConcurrentQueue<WallpaperInfo> _randomQueue = new();

    private async ValueTask<WallpaperInfo> RandomNextAsync()
    {
        if (_randomQueue.IsEmpty)
        {
            _logger.LogInformation("Wallpaper queue is empty, start get new randoms.");
            var infos = await _dbContext.WallpaperInfos.FromSqlRaw("SELECT * FROM wallpapers WHERE Enable ORDER BY RAND() LIMIT 720;").AsNoTracking().ToArrayAsync();
            foreach (var item in infos)
            {
                _randomQueue.Enqueue(item);
            }
            _logger.LogInformation("Enqueue finished.");
        }
        if (_randomQueue.TryDequeue(out var info))
        {
            return info;
        }
        else
        {
            return null!;
        }
    }




    public override void OnActionExecuted(ActionExecutedContext context)
    {
        base.OnActionExecuted(context);
        string? format = "";
        Version? version = null;
        if (context.HttpContext.Request.Query.TryGetValue("format", out var value1))
        {
            format = value1.FirstOrDefault()?.ToLower();
        }
        if (context.HttpContext.Request.Headers.TryGetValue("X-Version", out var value2))
        {
            if (Version.TryParse(value2.FirstOrDefault(), out version))
            {

            }
        }
        else
        {

        }
        if (version <= new Version(1, 3, 7, 0))
        {
            format = format switch
            {
                "jpeg" => "jpg",
                "avif" or "jpg" or "png" or "webp" => format,
                _ => "webp",
            };
        }
        else
        {
            format = format switch
            {
                "jpeg" => "jpg",
                "avif" or "jpg" or "png" or "webp" => format,
                _ => "avif",
            };
        }
        if (format is "jpg" or "png")
        {
            if (context.Result is ObjectResult res && res.Value is BaseWrapper wrapper)
            {
                if (wrapper.Data is WallpaperInfo info)
                {
                    info.Url = $"{info.Url}!{format}";
                    info.FileName = Path.ChangeExtension(info.FileName, format);
                }
                if (wrapper.Data is IWallpaperList wallpaperList)
                {
                    foreach (var item in wallpaperList.List)
                    {
                        item.Url = $"{item.Url}!{format}";
                        item.FileName = Path.ChangeExtension(item.FileName, format);
                    }
                }
            }
            if (context.Result is RedirectResult rr)
            {
                rr.Url = $"{rr.Url}!{format}";
            }
        }
        if (format is "avif" or "webp")
        {
            if (context.Result is ObjectResult res && res.Value is BaseWrapper wrapper)
            {
                if (wrapper.Data is WallpaperInfo info)
                {
                    info.Url = Path.ChangeExtension(info.Url, format);
                    info.FileName = Path.ChangeExtension(info.FileName, format);
                }
                if (wrapper.Data is IWallpaperList wallpaperList)
                {
                    foreach (var item in wallpaperList.List)
                    {
                        item.Url = Path.ChangeExtension(item.Url, format);
                        item.FileName = Path.ChangeExtension(item.FileName, format);
                    }
                }
            }
            if (context.Result is RedirectResult rr)
            {
                rr.Url = Path.ChangeExtension(rr.Url, format);
            }
        }
    }


}
