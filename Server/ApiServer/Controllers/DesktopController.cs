using Xunkong.ApiClient.Xunkong;

namespace Xunkong.ApiServer.Controllers;

[ApiController]
[ApiVersion("1")]
[Route("v{version:ApiVersion}/[controller]")]
//[ServiceFilter(typeof(BaseRecordResultFilter))]
[ResponseCache(Duration = 3600 * 24)]
public class DesktopController : Controller
{

    private readonly ILogger<DesktopController> _logger;

    private readonly XunkongDbContext _dbContext;

    private readonly DbConnectionFactory _factory;


    public DesktopController(ILogger<DesktopController> logger, XunkongDbContext dbContext, DbConnectionFactory factory)
    {
        _logger = logger;
        _dbContext = dbContext;
        _factory = factory;
    }



    [HttpGet("InfoBar")]
    public async Task<object> GetHomePageInfoBarsAsync([FromQuery(Name = "channel")] ChannelType? channel, [FromQuery(Name = "version")] Version? version)
    {
        if (channel == null)
        {
            channel = ChannelType.Sideload;
        }
        if (version == null)
        {
            version = new Version("1.0.0.0");
        }
        var vs = $"{version.Major:D2}.{version.Minor:D2}.{version.Build:D2}.{version.Revision:D2}";
        using var dapper = _factory.CreateDbConnection();
        var list = await dapper.QueryAsync<InfoBarContent>("SELECT * FROM infobarcontent WHERE Enable AND Channel=@Channel AND MinVersion<=@Version AND @Version<MaxVersion ORDER BY `Order`;", new { Channel = (int)channel, Version = vs });
        return new { List = list };
    }




}


