// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Xunkong.Desktop.Controls;
using Xunkong.GenshinData;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class CutscenePage : Page
{

    private readonly HttpClient httpClient;



    public CutscenePage()
    {
        this.InitializeComponent();
        httpClient = ServiceProvider.GetService<HttpClient>()!;
    }



    [ObservableProperty]
    private List<Cutscene> cutscenes;


    [ObservableProperty]
    private Cutscene selectedCutscene;

    partial void OnSelectedCutsceneChanged(Cutscene value)
    {
        if (value != null)
        {
            Grid_Info.Visibility = Visibility.Visible;
        }
    }



    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var str = await httpClient.GetStringAsync("https://file.xunkong.cc/genshin/cutscene/all.json");
            var list = JsonSerializer.Deserialize<List<Cutscene>>(str, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Cutscenes = list!.OrderByDescending(x => x.Version).ThenByDescending(x => x.Type).ThenBy(x => x.Group).ThenBy(x => x.Title).ThenBy(x => x.Player).ToList();
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
    }


    [RelayCommand]
    private void CloseInfoGrid()
    {
        Grid_Info.Visibility = Visibility.Collapsed;
    }




    private async void Button_PlayCutscene_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            if (button.DataContext is Cutscene cutscene)
            {
                try
                {
                    var folder = AppSetting.GetValue<string>(SettingKeys.CutsceneFolder);
                    if (!string.IsNullOrWhiteSpace(folder))
                    {
                        var path = Path.Combine(folder!, Path.GetFileNameWithoutExtension(cutscene.Source));
                        if (File.Exists(path) /*&& new FileInfo(path).Length == cutscene.Size*/)
                        {
                            var file = await StorageFile.GetFileFromPathAsync(path);
                            MainWindow.Current.SetFullWindowContent(new CutsceneViewer(file));
                            return;
                        }
                        path += ".mkv";
                        if (File.Exists(path) /*&& new FileInfo(path).Length == cutscene.Size*/)
                        {
                            var file = await StorageFile.GetFileFromPathAsync(path);
                            MainWindow.Current.SetFullWindowContent(new CutsceneViewer(file));
                            return;
                        }
                    }
                    MainWindow.Current.SetFullWindowContent(new CutsceneViewer(cutscene));
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    NotificationProvider.Error(ex);
                }
            }
        }
    }





    private void AppBarToggleButton_MutliSelction_Click(object sender, RoutedEventArgs e)
    {
        if (sender is AppBarToggleButton button)
        {
            if (button.IsChecked ?? false)
            {
                GridView_Cutscene.SelectionMode = ListViewSelectionMode.Multiple;
            }
            else
            {
                GridView_Cutscene.SelectionMode = ListViewSelectionMode.Single;
            }
        }
    }


    [RelayCommand]
    private async Task SetCutsceneFolderAsync()
    {
        try
        {
            var currentFolder = AppSetting.GetValue<string>(SettingKeys.CutsceneFolder);
            var dialog = new ContentDialog
            {
                Title = "本地文件夹",
                Content = $"如在本地文件夹中存在相同文件名（或加上 .mkv 扩展名）的文件，则播放过场动画时使用本地文件。动画文件需要自行下载。\r\n当前文件夹：\r\n{currentFolder}",
                PrimaryButtonText = "选择本地文件夹",
                SecondaryButtonText = "清除设置",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = MainWindow.Current.XamlRoot,
            };
            var result = await dialog.ShowAsync();
            if (result is ContentDialogResult.Primary)
            {
                var picker = new FolderPicker();
                WinRT.Interop.InitializeWithWindow.Initialize(picker, MainWindow.Current.HWND);
                var folder = await picker.PickSingleFolderAsync();
                if (folder != null)
                {
                    AppSetting.TrySetValue(SettingKeys.CutsceneFolder, folder.Path);
                }
            }
            if (result is ContentDialogResult.Secondary)
            {
                AppSetting.TrySetValue<string>(SettingKeys.CutsceneFolder, null!);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
    }




    [RelayCommand]
    private void CopyAllLinks()
    {
        try
        {
            var list = Cutscenes.Select(x => x.Source).ToList();
            var sb = new StringBuilder();
            foreach (var item in list)
            {
                sb.AppendLine(item);
            }
            ClipboardHelper.SetText(sb.ToString());
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
    }


    [RelayCommand]
    private void CopySelectionLinks()
    {
        try
        {
            var sb = new StringBuilder();
            foreach (var item in GridView_Cutscene.SelectedItems)
            {
                if (item is Cutscene cutscene)
                {
                    sb.AppendLine(cutscene.Source);
                }
            }
            ClipboardHelper.SetText(sb.ToString());
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
    }


    [RelayCommand]
    private async Task DownloadSelctedCutsceneAsync()
    {
        try
        {
            if (SelectedCutscene != null)
            {
                await Launcher.LaunchUriAsync(new Uri(SelectedCutscene.Source));
            }
        }
        catch { }
    }


    [RelayCommand]
    private void CopySelctedCutsceneLink()
    {
        try
        {
            if (SelectedCutscene != null)
            {
                ClipboardHelper.SetText(SelectedCutscene.Source);
            }
        }
        catch { }
    }



    [RelayCommand]
    private void DownloadWithPowerShell()
    {
        try
        {
            if (SelectedCutscene is null)
            {
                return;
            }
            var folder = AppSetting.GetValue<string>(SettingKeys.CutsceneFolder);
            if (string.IsNullOrWhiteSpace(folder))
            {
                NotificationProvider.Warning("没有设置本地文件夹");
                return;
            }
            Directory.CreateDirectory(folder);
            var file = Path.Combine(folder, Path.GetFileNameWithoutExtension(SelectedCutscene.Source) + ".mkv");
            if (File.Exists(file) && new FileInfo(file).Length == SelectedCutscene.Size)
            {
                NotificationProvider.Warning("文件已存在");
                return;
            }
            // https://www.powershellgallery.com/packages/Invoke-DownloadFile
            var script = $$""""
                $Url = \"{{SelectedCutscene.Source}}\"
                $Path = \"{{file}}\"

                function convertFileSize {
                    param(
                        $bytes
                    )

                    if ($bytes -lt 1MB) {
                        return \"$(\"{0:n2}\" -f [Math]::Round($bytes / 1KB, 2)) KB\"
                    }
                    elseif ($bytes -lt 1GB) {
                        return \"$(\"{0:n2}\" -f [Math]::Round($bytes / 1MB, 2)) MB\"
                    }
                    elseif ($bytes -lt 1TB) {
                        return \"$(\"{0:n2}\" -f [Math]::Round($bytes / 1GB, 2)) GB\"
                    }
                }

                Write-Verbose \"URL set to \"\"$($Url)\"\".\"

                if (!($Path)) {
                    Write-Verbose \"Path parameter not set, parsing Url for filename.\"
                    $URLParser = $Url | Select-String -Pattern \".*\:\/\/.*\/(.*\.{1}\w*).*\" -List

                    $Path = \"./$($URLParser.Matches.Groups[1].Value)\"
                }

                Write-Verbose \"Path set to \"\"$($Path)\"\".\"

                Write-Host \"Download file to \"\"$($Path)\"\"\"

                #Load in the WebClient object.
                Write-Verbose \"Loading in WebClient object.\"
                try {
                    $Downloader = New-Object -TypeName System.Net.WebClient
                    $Downloader.Headers.Add(\"User-Agent\", \"PowerShell NSPlayer WMFSDK\")
                }
                catch [Exception] {
                    Write-Host $_.Exception -ForegroundColor Red -ErrorAction Stop
                }

                #Creating a temporary file.
                $TmpFile = New-TemporaryFile
                Write-Verbose \"TmpFile set to \"\"$($TmpFile)\"\".\"

                try {

                    #Start the download by using WebClient.DownloadFileTaskAsync, since this lets us show progress on screen.
                    Write-Verbose \"Starting download...\"
                    $FileDownload = $Downloader.DownloadFileTaskAsync($Url, $TmpFile)

                    #Register the event from WebClient.DownloadProgressChanged to monitor download progress.
                    Write-Verbose \"Registering the \"\"DownloadProgressChanged\"\" event handle from the WebClient object.\"
                    Register-ObjectEvent -InputObject $Downloader -EventName DownloadProgressChanged -SourceIdentifier WebClient.DownloadProgressChanged | Out-Null

                    #Wait two seconds for the registration to fully complete
                    Start-Sleep -Seconds 3

                    if ($FileDownload.IsFaulted) {
                        Write-Verbose \"An error occurred. Generating error.\"
                        Write-Host $FileDownload.GetAwaiter().GetResult() -ForegroundColor Red -ErrorAction Stop
                        break
                    }

                    #While the download is showing as not complete, we keep looping to get event data.
                    while (!($FileDownload.IsCompleted)) {

                        if ($FileDownload.IsFaulted) {
                            Write-Verbose \"An error occurred. Generating error.\"
                            Write-Host $FileDownload.GetAwaiter().GetResult() -ForegroundColor Red -ErrorAction Stop
                            break
                        }

                        $EventData = Get-Event -SourceIdentifier WebClient.DownloadProgressChanged | Select-Object -ExpandProperty \"SourceEventArgs\" -Last 1

                        $ReceivedData = ($EventData | Select-Object -ExpandProperty \"BytesReceived\")
                        $TotalToReceive = ($EventData | Select-Object -ExpandProperty \"TotalBytesToReceive\")
                        $TotalPercent = $EventData | Select-Object -ExpandProperty \"ProgressPercentage\"

                        Write-Progress -Activity \"Downloading File\" -Status \"Percent Complete: $($TotalPercent)%\" -CurrentOperation \"Downloaded $(convertFileSize -bytes $ReceivedData) / $(convertFileSize -bytes $TotalToReceive)\" -PercentComplete $TotalPercent
                    }
                }
                catch [Exception] {
                    Write-Host $_.Exception.Message -ForegroundColor Red -ErrorAction Stop
                    Read-Host -Prompt \"ress Enter to exit\"
                }
                finally {
                    #Cleanup tasks
                    Write-Verbose \"Cleaning up...\"
                    Write-Progress -Activity \"Downloading File\" -Completed
                    Unregister-Event -SourceIdentifier WebClient.DownloadProgressChanged

                    if (($FileDownload.IsCompleted) -and !($FileDownload.IsFaulted)) {
                        #If the download was finished without termination, then we move the file.
                        Write-Verbose \"Moved the downloaded file to \"\"$($Path)\"\".\"
                        Move-Item -Path $TmpFile -Destination $Path -Force
                    }
                    else {
                        #If the download was terminated, we remove the file.
                        Write-Verbose \"Cancelling the download and removing the tmp file.\"
                        $Downloader.CancelAsync()
                        Remove-Item -Path $TmpFile -Force
                    }

                    $Downloader.Dispose()
                }
                """";
            Process.Start("PowerShell", script);
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
    }








}






