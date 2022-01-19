using System.ComponentModel;

namespace Xunkong.Core
{
    public static class EnumExtension
    {
        public static string ToDescription(this Enum enumValue)
        {
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

            var descriptionAttributes = fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            return descriptionAttributes?.Length > 0 ? descriptionAttributes[0].Description : enumValue.ToString();
        }

    }
}
