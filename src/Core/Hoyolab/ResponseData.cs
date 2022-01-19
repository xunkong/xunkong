namespace Xunkong.Core.Hoyolab
{
    internal class ResponseData<T> where T : class
    {

        [JsonPropertyName("retcode")]
        public int ReturnCode { get; set; }


        [JsonPropertyName("message")]
        public string? Message { get; set; }


        [JsonPropertyName("data")]
        public T? Data { get; set; }
    }
}
