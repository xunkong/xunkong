#pragma warning disable CS8618

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xunkong.Core.Wish
{
    [Table("Wishlog_Authkeys")]
    public class WishlogAuthkeyItem
    {
        [Key, MaxLength(4096)]
        public string Url { get; set; }

        public int Uid { get; set; }

        public DateTime DateTime { get; set; }

    }
}
