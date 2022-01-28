using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xunkong.Core.XunkongApi
{

    [Table("Desktop_Changelogs")]
    [Index(nameof(Version))]
    [Index(nameof(Channel))]
    public class DesktopChangelog
    {
        [JsonIgnore]
        public int Id { get; set; }

        [MaxLength(255)]
        public Version Version { get; set; }


        public ChannelType Channel { get; set; }


        public DateTimeOffset Time { get; set; }


        public ContentType ContentType { get; set; }


        public string Content { get; set; }


    }
}
