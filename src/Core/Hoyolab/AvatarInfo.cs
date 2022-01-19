using Xunkong.Core.Metadata;

namespace Xunkong.Core.Hoyolab
{
    public class AvatarInfo
    {

        [JsonPropertyName("id")]
        public int Id { get; set; }


        [JsonPropertyName("image")]
        public string Image { get; set; }


        [JsonPropertyName("name")]
        public string Name { get; set; }


        [JsonPropertyName("element"), JsonConverter(typeof(JsonStringEnumConverter))]
        public ElementType Element { get; set; }


        [JsonPropertyName("fetter")]
        public int Fetter { get; set; }


        [JsonPropertyName("level")]
        public int Level { get; set; }


        [JsonPropertyName("rarity")]
        public int Rarity { get; set; }


        [JsonPropertyName("actived_constellation_num")]
        public int ActivedConstellationNumber { get; set; }

    }
}
