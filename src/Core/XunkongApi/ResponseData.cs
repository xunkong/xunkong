namespace Xunkong.Core.XunkongApi
{
    public class ResponseData<TData>
    {
        public ReturnCode Code { get; set; }

        public string? Message { get; set; }

        public TData? Data { get; set; }


        public ResponseData() { }


        public ResponseData(ReturnCode code, string? message, TData data)
        {
            Code = code;
            Message = message ?? code.ToDescriptionOrString();
            Data = data;
        }


    }

    public class ResponseData
    {
        public ReturnCode Code { get; set; }

        public string? Message { get; set; }

        public object? Data { get; set; }


        public ResponseData() { }


        public ResponseData(ReturnCode code, string? message = null, object? data = null)
        {
            Code = code;
            Message = message ?? code.ToDescriptionOrString();
            Data = data;
        }



        public static ResponseData Ok(object data)
        {
            return new ResponseData(ReturnCode.Ok, null, data);
        }


    }
}
