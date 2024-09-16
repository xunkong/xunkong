namespace Xunkong.ApiClient;

public class GenshinDataWrapper<T>
{

    public GenshinDataWrapper() { }

    public GenshinDataWrapper(string language, int count, List<T> list)
    {
        Language = language;
        Count = count;
        List = list;
    }

    public string Language { get; set; }

    public int Count { get; set; }

    public List<T> List { get; set; }
}
