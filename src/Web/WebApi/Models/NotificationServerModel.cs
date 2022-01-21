using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Xunkong.Core.XunkongApi;

namespace Xunkong.Web.Api.Models
{
    [Table("Notifications")]
    [Index(nameof(Time))]
    [Index(nameof(Catagory))]
    [Index(nameof(Platform))]
    [Index(nameof(Channel))]
    [Index(nameof(MinVersion))]
    [Index(nameof(MaxVersion))]
    [Index(nameof(Enable))]
    public class NotificationServerModel : NotificationModelBase
    {

        [MaxLength(255)]
        public string Platform { get; set; }

        [MaxLength(255)]
        public string Channel { get; set; }

        [MaxLength(255)]
        public string MinVersion { get; set; }

        [MaxLength(255)]
        public string MaxVersion { get; set; }

        public bool Enable { get; set; }


    }
}
