using System.ComponentModel.DataAnnotations.Schema;

namespace Xunkong.ApiServer.Models;

[Table("Record_Wishlog")]
[Index(nameof(Uid))]
[Index(nameof(Operation))]
public class WishlogRecordModel : BaseRecordModel
{

    public int Uid { get; set; }

    public string? Operation { get; set; }

    public int CurrentCount { get; set; }

    public int GetCount { get; set; }

    public int PutCount { get; set; }

    public int DeleteCount { get; set; }

}
