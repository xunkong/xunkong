namespace Xunkong.Core.XunkongApi
{
    public class XunkongException : Exception
    {
        public ErrorCode Code { get; init; }


        public XunkongException() { }


        public XunkongException(ErrorCode code, string? message = null) : base(message ?? code.ToDescriptionOrString())
        {
            Code = code;
        }

    }
}
