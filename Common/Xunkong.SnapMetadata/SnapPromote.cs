using System.Text.Json.Serialization;

namespace Xunkong.SnapMetadata;

public class SnapPromote
{

    [JsonPropertyName("Id")]
    public int PromoteId { get; set; }


    public int Level { get; set; }


    public List<SnapPromoteAddProperty> AddProperties { get; set; }


}



public class SnapPromoteAddProperty
{

    public int Type { get; set; }

    public double Value { get; set; }

}