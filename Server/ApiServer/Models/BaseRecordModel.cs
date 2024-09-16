using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xunkong.ApiServer.Models;

[Table("Record_All")]
[Index(nameof(DateTime))]
[Index(nameof(Path))]
[Index(nameof(StatusCode))]
[Index(nameof(ReturnCode))]
[Index(nameof(DeviceId))]
[Index(nameof(Ip))]
[Index(nameof(Platform))]
[Index(nameof(Channel))]
[Index(nameof(Version))]
public class BaseRecordModel
{

    [Key]
    public string RequestId { get; set; }

    public DateTimeOffset DateTime { get; set; }

    [MaxLength(255)]
    public string? Path { get; set; }

    [MaxLength(255)]
    public string? Method { get; set; }

    public int StatusCode { get; set; }

    public ErrorCode? ReturnCode { get; set; }

    public string? Message { get; set; }

    [MaxLength(255)]
    public string? DeviceId { get; set; }

    public string? UserAgent { get; set; }

    [MaxLength(255)]
    public string? Ip { get; set; }

    [MaxLength(255)]
    public string? Platform { get; set; }

    [MaxLength(255)]
    public string? Channel { get; set; }

    [MaxLength(255)]
    public string? Version { get; set; }


}
