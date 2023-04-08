using System.Data;
using Xunkong.Hoyolab.TravelNotes;

namespace Xunkong.Desktop.Helpers;

internal class DapperSqlMapper
{

    private static JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping, PropertyNameCaseInsensitive = true };

    public class DateTimeOffsetHandler : SqlMapper.TypeHandler<DateTimeOffset>
    {
        public override DateTimeOffset Parse(object value)
        {
            if (value is string str)
            {
                return DateTimeOffset.Parse(str);
            }
            else
            {
                return new DateTimeOffset();
            }
        }

        public override void SetValue(IDbDataParameter parameter, DateTimeOffset value)
        {
            parameter.Value = value.ToString();
        }
    }



    public class TravelNotesPrimogemsMonthGroupStatsListHandler : SqlMapper.TypeHandler<List<TravelNotesPrimogemsMonthGroupStats>>
    {
        public override List<TravelNotesPrimogemsMonthGroupStats> Parse(object value)
        {
            if (value is string str)
            {
                if (!string.IsNullOrWhiteSpace(str))
                {
                    return JsonSerializer.Deserialize<List<TravelNotesPrimogemsMonthGroupStats>>(str)!;
                }
            }
            return new();
        }

        public override void SetValue(IDbDataParameter parameter, List<TravelNotesPrimogemsMonthGroupStats> value)
        {
            parameter.Value = JsonSerializer.Serialize(value);
        }
    }

    public class StringListHandler : SqlMapper.TypeHandler<List<string>>
    {
        public override List<string> Parse(object value)
        {
            if (value is string str)
            {
                if (!string.IsNullOrWhiteSpace(str))
                {
                    return JsonSerializer.Deserialize<List<string>>(str)!;
                }
            }
            return new();
        }

        public override void SetValue(IDbDataParameter parameter, List<string> value)
        {
            parameter.Value = JsonSerializer.Serialize(value, JsonSerializerOptions);
        }
    }


}
