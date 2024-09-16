using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Xunkong.ApiClient;

namespace Xunkong.Desktop.Models;

public class WallpaperInfoEx : WallpaperInfo
{

    public int MyRating { get; set; } = -1;

    public DateTimeOffset Time { get; set; }

    [JsonIgnore]
    public string SourceDomain => new Uri(Source ?? "").Host;

    [JsonIgnore]
    public string RatingText => $"{Math.Clamp(Rating, 0, 5):F1}/{RatingCount}";

    [JsonIgnore]
    public string Thumb => GetThumb();

    [JsonIgnore]
    public bool ShowMyRating => MyRating > 0;

    [JsonIgnore]
    private static readonly Regex regex = new Regex("\\?", RegexOptions.Compiled);

    private string GetThumb()
    {
        if (Url?.Contains("xunkong.cc") ?? false)
        {
            if (Regex.IsMatch(Url, "![^/?]+"))
            {
                return Regex.Replace(Url, "![^/?]+", "!thumb");
            }
            else if (Url.Contains("?"))
            {
                return regex.Replace(Url, "!thumb?", 1);
            }
            else
            {
                return $"{Url}!thumb";
            }
        }
        else
        {
            return Url!;
        }
    }

    public static WallpaperInfoEx FromWallpaper(WallpaperInfo info)
    {
        return new WallpaperInfoEx
        {
            Author = info.Author,
            Description = info.Description,
            Enable = info.Enable,
            FileName = info.FileName,
            Id = info.Id,
            Rating = info.Rating,
            RatingCount = info.RatingCount,
            Source = info.Source,
            Tags = info.Tags,
            Title = info.Title,
            Url = info.Url,
        };
    }


    public static WallpaperInfoEx FromUri(string uri)
    {
        return new WallpaperInfoEx { Url = uri };
    }

}
