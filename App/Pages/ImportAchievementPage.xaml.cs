using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Web;
using Windows.ApplicationModel.Activation;
using Xunkong.GenshinData.Achievement;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class ImportAchievementPage : Page
{


    private string? dataFrom;

    private string? dataParam;

    [ObservableProperty]
    private string? exportApp;

    [ObservableProperty]
    private string? exportAppVersion;

    [ObservableProperty]
    private string? uiafVersion;


    private List<AchievementData>? achievementDatas;

    [ObservableProperty]
    private string error;

    [ObservableProperty]
    private int achievementCount;


    private List<string> uids;


    public ImportAchievementPage()
    {
        this.InitializeComponent();
        Loaded += ImportAchievementPage_Loaded;
    }



    public ImportAchievementPage(IProtocolActivatedEventArgs args)
    {
        this.InitializeComponent();
        ToolWindow.Current.ResizeToCenter(360, 480);
        InitializeTitle(args);
        Loaded += ImportAchievementPage_Loaded;
    }




    private void InitializeTitle(IProtocolActivatedEventArgs args)
    {
        if (args is ProtocolActivatedEventArgs args1)
        {
            var qc = HttpUtility.ParseQueryString(args1.Uri.Query);
            var caller = qc["caller"];
            c_TextBlock_Title.Text = "导入成就";
            c_TextBlock_Caller.Text = $"调用方：{(string.IsNullOrWhiteSpace(caller) ? "未知" : caller)}";
            dataFrom = qc["from"]?.ToString();
            if (dataFrom == "file")
            {
                dataParam = qc["path"]?.ToString().Replace("\"", "").Trim();
            }
            if (dataFrom == "web")
            {
                dataParam = qc["url"]?.ToString().Replace("\"", "").Trim();
            }
        }
    }



    private async void ImportAchievementPage_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            string? text = null;
            if (dataFrom?.ToLower() is "clipboard")
            {
                text = await ClipboardHelper.GetTextAsync();
            }
            if (dataFrom?.ToLower() is "file")
            {
                if (File.Exists(dataParam))
                {
                    text = await File.ReadAllTextAsync(dataParam);
                }
            }
            if (dataParam?.ToLower() is "web")
            {
                text = await CreateHttpClient().GetStringAsync(dataParam);
            }
            ParseJsonString(text);
            if (achievementDatas?.Any() ?? false)
            {
                AchievementCount = achievementDatas.Count;
                using var dapper = DatabaseProvider.CreateConnection();
                uids = dapper.Query<string>("SELECT DISTINCT Uid FROM AchievementData;").ToList();
                c_AutoSuggestBox_Uid.ItemsSource = uids;
            }
            else
            {
                Error = "没有可导入的成就";
            }
        }
        catch (Exception ex)
        {
            Error = "数据解析失败";
            Logger.Error(ex, "成就数据解析失败");
        }
    }



    private void ParseJsonString(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }
        var baseNode = JsonNode.Parse(text);
        if (baseNode?["info"] is JsonNode infoNode)
        {
            ExportApp = infoNode?["export_app"]?.ToString();
            ExportAppVersion = infoNode?["export_app_version"]?.ToString();
            UiafVersion = infoNode?["uiaf_version"]?.ToString();
        }
        if (baseNode?["list"] is JsonNode listNode)
        {
            achievementDatas = JsonSerializer.Deserialize<List<AchievementData>>(listNode);
        }
    }



    private HttpClient CreateHttpClient()
    {
        var handler = new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.All };
        var client = new HttpClient(handler);
        client.DefaultRequestHeaders.Add("User-Agent", $"Xunkong/{XunkongEnvironment.AppName}");
        return client;
    }


    private void c_AutoSuggestBox_Uid_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            if (string.IsNullOrWhiteSpace(sender.Text))
            {
                sender.ItemsSource = uids;
            }
            else
            {
                sender.ItemsSource = uids?.Where(x => x.StartsWith(sender.Text)).ToList();
            }
        }
    }



    private async void c_Button_Import_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (int.TryParse(c_AutoSuggestBox_Uid.Text, out int uid))
            {
                if (achievementDatas != null)
                {
                    c_Button_Import.Content = "导入中，请稍等";
                    c_Button_Import.IsEnabled = false;
                    await Task.Delay(100);
                    var now = DateTimeOffset.Now;
                    foreach (var item in achievementDatas)
                    {
                        item.Uid = uid;
                        item.LastUpdateTime = now;
                        if (item.Status == 0)
                        {
                            item.Status = 3;
                        }
                    }
                    using var dapper = DatabaseProvider.CreateConnection();
                    dapper.Execute("INSERT OR REPLACE INTO AchievementData (Uid, Id, Current, Status, FinishedTime, LastUpdateTime) VALUES (@Uid, @Id, @Current, @Status, @FinishedTime, @LastUpdateTime);", achievementDatas);
                    c_Button_Import.Content = "导入完成";
                }
            }
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            Logger.Error(ex, "导入成就数据");
        }
    }
}
