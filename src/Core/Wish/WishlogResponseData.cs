namespace Xunkong.Core.Wish
{
    internal class WishlogResponseData
    {
        [JsonPropertyName("retcode")]
        public int Retcode { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public DataItem? Data { get; set; }

        public class DataItem
        {
            [JsonPropertyName("page"), JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
            public int Page { get; set; }

            [JsonPropertyName("size"), JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
            public int Size { get; set; }

            [JsonPropertyName("total"), JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
            public int Total { get; set; }

            [JsonPropertyName("list")]
            public List<WishlogItem>? List { get; set; }

            [JsonPropertyName("region")]
            public string? Region { get; set; }
        }
    }
}
