namespace Xunkong.Desktop.Models;

internal class OperationHistory
{

    public int Id { get; set; }

    public DateTimeOffset Time { get; set; }

    public string Operation { get; set; }

    public string? Key { get; set; }

    public string? Value { get; set; }


    public static void AddToDatabase(string operation, string? key = null, object? value = null)
    {
        try
        {
            var op = new OperationHistory { Time = DateTimeOffset.Now, Operation = operation, Key = key };
            if (value is not null)
            {
                op.Value = JsonSerializer.Serialize(value, JsonSerializerOptions);
            }
            using var dapper = DatabaseProvider.CreateConnection();
            dapper.Execute("INSERT INTO OperationHistory (Time, Operation, Key, Value) VALUES (@Time, @Operation, @Key, @Value);", op);
        }
        catch { }
    }


    private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

}
