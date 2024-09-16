using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xunkong.ApiServer.Models;

[Table("wishlog_authkeys")]
public class WishlogAuthkeyItem
{
    [Key]
    public int Uid { get; set; }

    public string Url { get; set; }

    public DateTime DateTime { get; set; }

}