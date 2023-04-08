using System.Text.Json.Serialization;
using Xunkong.ApiClient;

namespace Xunkong.Desktop.Models;

public class WallpaperInfoEx : WallpaperInfo
{

    public int MyRating { get; set; } = -1;

    [JsonIgnore]
    public string SourceDomain => new Uri(Source ?? "").Host;

    [JsonIgnore]
    public string RatingText => $"{Math.Clamp(Rating, 0, 5):F1}/{RatingCount}";

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

}
