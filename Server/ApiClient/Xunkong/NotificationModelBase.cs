namespace Xunkong.ApiClient;

public class NotificationModelBase
{

    public int Id { get; set; }

    public DateTimeOffset Time { get; set; }

    public string Title { get; set; }

    public string? Category { get; set; }

    public NotificationContentType ContentType { get; set; }

    public string Content { get; set; }

}
