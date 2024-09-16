namespace Xunkong.ApiServer.Models;

public class XunkongApiServerException : Exception
{
    public ErrorCode Code { get; set; }

    public XunkongApiServerException(ErrorCode code, string? message = null) : base(message ?? code.ToDescription())
    {
        Code = code;
    }

}
