using Microsoft.AspNetCore.Mvc;
using Xunkong.Core.XunkongApi;

namespace Xunkong.Web.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ApiVersion("0")]
    [ApiVersionNeutral]
    public class StateController : ControllerBase
    {
        public static DateTime StartTime;

        private static DateTime lastUpdateTime = new FileInfo(typeof(StateController).Assembly.Location).CreationTime;


        /// <summary>
        /// 返回函数实例的状态，并保持函数实例长时间运行
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ResponseData GetState()
        {
            var data = new ResponseData(ReturnCode.Ok, "Api instance has started.", new { LastUpdateTime = lastUpdateTime, RunningTime = DateTime.Now - StartTime });
            return data;
        }

        [HttpGet("long")]
        public string GetLong()
        {
            var str = System.IO.File.ReadAllText(@"E:\main_wishlog.sql");
            return str;
        }

    }
}
