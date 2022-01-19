using Xunkong.Core.Hoyolab;

namespace Xunkong.Core.Wish
{
    [Table("Info_WishEvent")]
    [Index(nameof(WishType))]
    [Index(nameof(WishType), nameof(_StartTimeString), IsUnique = true)]
    public class WishEventInfo
    {

        public int Id { get; set; }


        public WishType WishType { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Version { get; set; }


        [JsonIgnore, NotMapped]
        public DateTimeOffset StartTime => TimeStringToTimeOffset(_StartTimeString);


        [JsonIgnore, NotMapped]
        public DateTimeOffset EndTime => TimeStringToTimeOffset(_EndTimeString);


        [JsonPropertyName("StartTime"), JsonInclude]
        [Column("StartTime"), MaxLength(255)]
        public string _StartTimeString { get; private set; }


        [JsonPropertyName("EndTime"), JsonInclude]
        [Column("EndTime"), MaxLength(255)]
        public string _EndTimeString { get; private set; }

        /// <summary>
        /// Up5星物品，此值不要使用SQL查询
        /// </summary>
        public List<string> Rank5UpItems { get; set; }

        /// <summary>
        /// Up4星物品，此值不要使用SQL查询
        /// </summary>
        public List<string> Rank4UpItems { get; set; }



        [JsonIgnore, NotMapped]
        public string DisplayName
        {
            get
            {
                return $"{Version:F1} {Name}";
            }
        }

        [JsonIgnore, NotMapped]
        public string UpItems
        {
            get
            {
                string result = "";
                foreach (var item in Rank5UpItems)
                {
                    result += $" {item}";
                }
                foreach (var item in Rank4UpItems)
                {
                    result += $" {item}";
                }
                return result.Trim();
            }
        }



        [JsonIgnore, NotMapped]
        public static RegionType RegionType { get; set; }


        private static DateTimeOffset TimeStringToTimeOffset(string str)
        {
            if (str.Contains("+") || str.Contains("-"))
            {
                return DateTimeOffset.Parse(str);
            }
            else
            {
                var offset = RegionType switch
                {
                    RegionType.US => "-05:00",
                    RegionType.EU => "+01:00",
                    _ => "+08:00",
                };
                return DateTimeOffset.Parse($"{str} {offset}");
            }
        }


    }
}
