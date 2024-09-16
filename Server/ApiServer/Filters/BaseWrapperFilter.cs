using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Xunkong.ApiServer.Filters;

// https://www.cnblogs.com/wucy/p/16124449.html
public class BaseWrapperFilter : IActionFilter
{
    public void OnActionExecuted(ActionExecutedContext context)
    {
        var controllerActionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
        var actionWrapper = controllerActionDescriptor?.MethodInfo.GetCustomAttributes(typeof(NoWrapperAttribute), false).FirstOrDefault();
        var controllerWrapper = controllerActionDescriptor?.ControllerTypeInfo.GetCustomAttributes(typeof(NoWrapperAttribute), false).FirstOrDefault();
        //如果包含NoWrapperAttribute则说明不需要对返回结果进行包装，直接返回原始值
        if (actionWrapper != null || controllerWrapper != null)
        {
            return;
        }
        if (context.Result is ObjectResult result)
        {
            var wrapper = new BaseWrapper { Code = 0, Message = "OK", Data = result.Value };
            context.Result = new ObjectResult(wrapper);
        }
        if (context.Result is EmptyResult)
        {
            context.Result = new ObjectResult(new { Code = 0, Message = "OK", Data = new object() });
        }
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
    }
}
