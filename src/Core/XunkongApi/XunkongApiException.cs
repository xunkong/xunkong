namespace Xunkong.Core.XunkongApi
{
    public class XunkongApiException : Exception
    {
        public ReturnCode Code { get; init; }


        public XunkongApiException() { }


        public XunkongApiException(ReturnCode code, string? message = null) : base(message ?? code.ToDescriptionOrString())
        {
            Code = code;
        }

    }
}
