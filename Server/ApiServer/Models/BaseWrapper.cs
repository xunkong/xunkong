namespace Xunkong.ApiServer.Models;

public class BaseWrapper<T>
{

    public int Code { get; set; }

    public string Message { get; set; }

    public T? Data { get; set; }

}


public class BaseWrapper
{

    public int Code { get; set; }

    public string Message { get; set; }

    public object? Data { get; set; }

}
