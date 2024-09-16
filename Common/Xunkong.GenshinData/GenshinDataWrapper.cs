namespace Xunkong.GenshinData;

public record GenshinDataWrapper<T>(string Language, int Count, IEnumerable<T> List);
