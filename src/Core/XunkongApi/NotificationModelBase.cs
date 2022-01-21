using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xunkong.Core.XunkongApi
{
    public class NotificationModelBase
    {

        public int Id { get; set; }

        public DateTimeOffset Time { get; set; }

        [MaxLength(255)]
        public string Title { get; set; }

        [MaxLength(255)]
        public string Catagory { get; set; }

        public NotificationType ContentType { get; set; }

        public string Content { get; set; }


    }



    public enum NotificationType
    {

        Text,

        Markdown,

        Html,

        Url,

    }


}
