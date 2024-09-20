using System.Text.Json.Serialization;

namespace Xunkong.SnapMetadata;

public class SnapAvatarPromote
{

    [JsonPropertyName("Id")]
    public int PromoteId { get; set; }


    public int Level { get; set; }


    public List<SnapAvatarPromoteAddProperty> AddProperties { get; set; }


}



public class SnapAvatarPromoteAddProperty
{

    public int Type { get; set; }

    public double Value { get; set; }

}