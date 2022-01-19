using System.ComponentModel;

namespace Xunkong.Core.XunkongApi
{
    public static class EnumExtension
    {
        public static string ToDescriptionOrString(this Enum item)
        {
            string text = item.ToString();
            return (item.GetType().GetField(text)?.GetCustomAttributes(typeof(DescriptionAttribute), inherit: false)?.FirstOrDefault() as DescriptionAttribute)?.Description ?? text;
        }

    }
}
