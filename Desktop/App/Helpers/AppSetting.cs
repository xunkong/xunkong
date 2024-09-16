using System.ComponentModel;
using Windows.Storage;

namespace Xunkong.Desktop.Helpers;

internal static class AppSetting
{

    private static readonly ApplicationDataContainer _container;

    static AppSetting()
    {
        _container = ApplicationData.Current.LocalSettings;
    }


    public static T? GetValue<T>(string key, T? defaultValue = default)
    {
        try
        {
            var value = _container.Values[key];
            if (value is T t)
            {
                return t;
            }
            else if (value is string s)
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter == null)
                {
                    return defaultValue;
                }
                return (T?)converter.ConvertFromString(s);
            }
            return defaultValue;
        }
        catch
        {
            return defaultValue;
        }

    }


    public static void SetValue<T>(string key, T value)
    {
        try
        {
            _container.Values[key] = value?.ToString();
        }
        catch { }
    }


    public static bool TryGetValue<T>(string key, out T? result, T? defaultValue = default)
    {
        try
        {
            var value = _container.Values[key];
            if (value is T t)
            {
                result = t;
                return true;
            }
            else if (value is string s)
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter is null)
                {
                    result = defaultValue;
                    return false;
                }
                else
                {
                    result = (T?)converter.ConvertFromString(s);
                    return true;
                }
            }
            result = defaultValue;
            return false;
        }
        catch
        {
            result = defaultValue;
            return false;
        }
    }


}
