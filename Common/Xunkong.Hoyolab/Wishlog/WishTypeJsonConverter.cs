namespace Xunkong.Hoyolab.Wishlog;

public class WishTypeJsonConverter : JsonConverter<WishType>
{
    public override WishType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        if (int.TryParse(str, out var num))
        {
            return (WishType)num;
        }
        else
        {
            return 0;
        }
    }

    public override void Write(Utf8JsonWriter writer, WishType value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(((int)value).ToString());
    }
}
