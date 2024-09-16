namespace Xunkong.ApiClient;

public class XunkongApiException : XunkongException
{

    public int Code { get; init; }


    public XunkongApiException() { }


    public XunkongApiException(int code, string? message = null) : base(message)
    {
        Code = code;
    }

}
