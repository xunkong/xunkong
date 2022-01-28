using System.ComponentModel;

namespace Xunkong.Core
{
    public static class EnumExtension
    {
        public static string ToDescriptionOrString(this Enum enumValue)
        {
            string text = enumValue.ToString();
            var fieldInfo = enumValue.GetType().GetField(text);
            var descriptionAttribute = fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault();
            return (descriptionAttribute as DescriptionAttribute)?.Description ?? text;
        }

    }
}
