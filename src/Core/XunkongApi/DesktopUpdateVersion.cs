using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xunkong.Core.XunkongApi
{

    [Table("Desktop_UpdateVersions")]
    [Index(nameof(Version))]
    [Index(nameof(Channel))]
    public class DesktopUpdateVersion
    {

        public int Id { get; set; }

        public Version Version { get; set; }


        public ChannelType Channel { get; set; }


        public DateTimeOffset Time { get; set; }


        public string? Abstract { get; set; }


        public string? PackageUrl { get; set; }

    }
}
