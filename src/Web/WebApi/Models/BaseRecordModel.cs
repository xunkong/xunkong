using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Xunkong.Core.XunkongApi;

namespace Xunkong.Web.Api.Models
{
    [Table("Record_All")]
    [Index(nameof(DateTime))]
    [Index(nameof(Path))]
    [Index(nameof(StatusCode))]
    [Index(nameof(ReturnCode))]
    [Index(nameof(DeviceId))]
    [Index(nameof(Ip))]
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

        public ReturnCode? ReturnCode { get; set; }

        public string? Message { get; set; }

        [MaxLength(255)]
        public string? DeviceId { get; set; }

        public string? UserAgent { get; set; }

        [MaxLength(255)]
        public string? Ip { get; set; }


    }
}
