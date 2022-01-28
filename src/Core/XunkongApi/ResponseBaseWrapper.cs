namespace Xunkong.Core.XunkongApi
{
    public class ResponseBaseWrapper<TData>
    {
        public ErrorCode Code { get; set; }

        public string? Message { get; set; }

        public TData? Data { get; set; }


        public ResponseBaseWrapper() { }


        public ResponseBaseWrapper(ErrorCode code, string? message, TData data)
        {
            Code = code;
            Message = message ?? code.ToDescriptionOrString();
            Data = data;
        }


    }

    public class ResponseBaseWrapper
    {
        public ErrorCode Code { get; set; }

        public string? Message { get; set; }

        public object? Data { get; set; }


        public ResponseBaseWrapper() { }


        public ResponseBaseWrapper(ErrorCode code, string? message = null, object? data = null)
        {
            Code = code;
            Message = message ?? code.ToDescriptionOrString();
            Data = data;
        }



        public static ResponseBaseWrapper Ok(object data)
        {
            return new ResponseBaseWrapper(ErrorCode.Ok, null, data);
        }


    }
}
