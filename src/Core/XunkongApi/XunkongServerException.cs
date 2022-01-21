namespace Xunkong.Core.XunkongApi
{
    public class XunkongServerException : Exception
    {
        public ReturnCode Code { get; init; }


        public XunkongServerException() { }


        public XunkongServerException(ReturnCode code, string? message = null) : base(message ?? code.ToDescriptionOrString())
        {
            Code = code;
        }

    }
}
