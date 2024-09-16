namespace Xunkong.Hoyolab.Wishlog;

internal class ItemIdJsonConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.Number)
        {
            return reader.GetInt32();
        }
        if ((reader.TokenType is JsonTokenType.String))
        {
            var str = reader.GetString();
            if (int.TryParse(str, out var id))
            {
                return id;
            }
        }
        return 0;
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}