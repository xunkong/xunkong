using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Xunkong.ApiServer.Models;

[Table("Notifications")]
[Index(nameof(Time))]
[Index(nameof(Category))]
[Index(nameof(Platform))]
[Index(nameof(Channel))]
[Index(nameof(MinVersion))]
[Index(nameof(MaxVersion))]
[Index(nameof(Enable))]
public class NotificationServerModel : NotificationModelBase
{
    [JsonIgnore]
    public PlatformType Platform { get; set; }

    [JsonIgnore]
    public ChannelType Channel { get; set; }

    [MaxLength(255), JsonIgnore]
    public Version? MinVersion { get; set; }

    [MaxLength(255), JsonIgnore]
    public Version? MaxVersion { get; set; }

    [JsonIgnore]
    public bool Enable { get; set; }


}
