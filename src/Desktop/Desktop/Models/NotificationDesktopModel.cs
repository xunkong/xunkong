using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using Xunkong.Core.XunkongApi;

namespace Xunkong.Desktop.Models
{

    [Table("Notifications")]
    [Index(nameof(Catagory))]
    [Index(nameof(HasRead))]
    internal class NotificationDesktopModel : NotificationModelBase
    {

        public bool HasRead { get; set; }

    }
}
