// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Xunkong.ApiServer.Controllers;

/// <summary>
/// 返回值
/// </summary>
[ApiController]
[ApiVersion("1")]
[Route("v{version:ApiVersion}/[controller]")]
public class ReturnCodeController : ControllerBase
{



    /// <summary>
    /// 获取返回值格式，以及状态码列表
    /// </summary>
    /// <returns>retcode</returns>
    [HttpGet]
    [ResponseCache(Duration = 86400)]
    public object Get()
    {
        var dic = Enum.GetValues<ErrorCode>().ToDictionary(x => (int)x, x => x.ToDescription());
        return new { CodeCount = dic.Count, Dic = dic };
    }

}
