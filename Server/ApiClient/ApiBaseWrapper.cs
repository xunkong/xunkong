namespace Xunkong.ApiClient;

internal record ApiBaseWrapper<T>(int Code, string? Message, T Data);
