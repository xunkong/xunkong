// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Text;
using System.Text.RegularExpressions;
using Xunkong.Desktop.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class WallpaperHistoryPage : Page
{

    public WallpaperHistoryPage()
    {
        this.InitializeComponent();
        Loaded += WallpaperHistoryPage_Loaded;
    }


    private List<WallpaperInfoEx> wallpaperResult;

    [ObservableProperty]
    private List<WallpaperInfoEx> wallpaperInfoHistorys;



    private async void WallpaperHistoryPage_Loaded(object sender, RoutedEventArgs e)
    {
        await Task.Delay(30);
        SearchOrLoadHistory();
    }




    private void GridView_Images_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is WallpaperInfoEx info)
        {
            MainWindow.Current.SetFullWindowContent(new ImageViewer { CurrentImage = info, ImageCollection = WallpaperInfoHistorys, DecodeFromStream = true, ShowLoadingRing = true });
        }
    }




    private void AutoSuggestBox_Search_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        SearchOrLoadHistory(sender.Text);
    }


    private void SearchOrLoadHistory(string? text = null)
    {
        try
        {
            using var dapper = DatabaseProvider.CreateConnection();
            if (string.IsNullOrWhiteSpace(text))
            {
                wallpaperResult = dapper.Query<WallpaperInfoEx>("""
                SELECT wi.*, wr.Rating AS MyRating, wh.Time FROM WallpaperHistory AS wh
                INNER JOIN WallpaperInfo AS wi ON wh.WallpaperId = wi.Id
                LEFT JOIN WallpaperRating AS wr ON wh.WallpaperId = wr.WallpaperId
                WHERE wh.Id IN (SELECT MAX(Id) FROM WallpaperHistory GROUP BY WallpaperId)
                ORDER BY wh.Id DESC;
                """).ToList();
            }
            else
            {
                OperationHistory.AddToDatabase("SearchHistoricalWallpaper", text);
                Logger.TrackEvent("SearchHistoricalWallpaper", "Words", text);
                var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var sb = new StringBuilder();
                var dp = new DynamicParameters();
                dp.Add("@regex", string.Join("|", words.Select(Regex.Escape)));
                sb.Append("SELECT wi.*, wr.Rating AS MyRating, wh.Time, (");
                for (int i = 0; i < words.Length; i++)
                {
                    sb.Append($"IIF((Title || ' ' || Author || ' ' || Description || ' ' || Tags) LIKE @p{i}, 1, 0) + ");
                    dp.Add($"@p{i}", $"%{words[i]}%");
                }
                sb.Append($"""
                0) AS weight 
                FROM WallpaperInfo AS wi LEFT JOIN WallpaperRating AS wr ON wi.Id = WR.WallpaperId
                LEFT JOIN (SELECT * FROM WallpaperHistory WHERE Id IN (SELECT MAX(Id) FROM WallpaperHistory GROUP BY WallpaperId)) AS wh ON wi.Id = wh.WallpaperId
                WHERE (Title || ' ' || Author || ' ' || Description || ' ' || Tags) REGEXP @regex ORDER BY weight DESC, IIF(wr.Rating > 0, wr.Rating, 3) DESC, wi.Rating DESC;
                """);
                dapper.CreateFunction("REGEXP", (string pattern, string input) => string.IsNullOrWhiteSpace(input) ? false : Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant));
                wallpaperResult = dapper.Query<WallpaperInfoEx>(sb.ToString(), dp).ToList();
            }
            OrderResultList();
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
    }



    private void RadioButtons_Order_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            OrderResultList();
        }
        catch { }
    }


    private void OrderResultList()
    {
        if (wallpaperResult != null)
        {
            WallpaperInfoHistorys = RadioButtons_Order.SelectedIndex switch
            {
                1 => wallpaperResult.OrderByDescending(x => x.Time).ToList(),
                2 => wallpaperResult.OrderByDescending(x => x.MyRating).ToList(),
                3 => wallpaperResult.OrderByDescending(x => x.Rating).ToList(),
                _ => wallpaperResult,
            };
        }
    }


}
