namespace Xunkong.ApiServer.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
internal class NoWrapperAttribute : Attribute
{

}