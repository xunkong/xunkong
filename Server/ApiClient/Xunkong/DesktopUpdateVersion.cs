namespace Xunkong.ApiClient;

public class DesktopUpdateVersion
{

    public int Id { get; set; }

    public Version Version { get; set; }


    public ChannelType Channel { get; set; }


    public DateTimeOffset Time { get; set; }


    public string? Abstract { get; set; }


    public string? PackageUrl { get; set; }

}
