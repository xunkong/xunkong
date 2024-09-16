namespace Xunkong.ApiClient;

internal class ListWrapper<T>
{
    [JsonPropertyName("list")]
    public List<T> List { get; set; }
}
