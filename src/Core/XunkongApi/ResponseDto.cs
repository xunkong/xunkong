namespace Xunkong.Core.XunkongApi
{
    public class ResponseDto<TData>
    {
        public ReturnCode Code { get; set; }

        public string? Message { get; set; }

        public TData? Data { get; set; }


        public ResponseDto() { }


        public ResponseDto(ReturnCode code, string? message, TData data)
        {
            Code = code;
            Message = message ?? code.ToDescriptionOrString();
            Data = data;
        }


    }

    public class ResponseDto
    {
        public ReturnCode Code { get; set; }

        public string? Message { get; set; }

        public object? Data { get; set; }


        public ResponseDto() { }


        public ResponseDto(ReturnCode code, string? message = null, object? data = null)
        {
            Code = code;
            Message = message ?? code.ToDescriptionOrString();
            Data = data;
        }



        public static ResponseDto Ok(object data)
        {
            return new ResponseDto(ReturnCode.Ok, null, data);
        }


    }
}
