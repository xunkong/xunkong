// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Net.Http;
using System.Text;
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
        httpClient = new HttpClient(new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.All });
        httpClient.DefaultRequestHeaders.Add("User-Agent", $"XunkongDesktop/{XunkongEnvironment.AppVersion}");
    }



    [ObservableProperty]
    private List<Cutscene> cutscenes;


    [ObservableProperty]
    private Cutscene selectedCutscene;





    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var str = await httpClient.GetStringAsync("https://file.xunkong.cc/genshin/cutscene/all.json");
            var list = JsonSerializer.Deserialize<List<Cutscene>>(str, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Cutscenes = list!.OrderByDescending(x => x.Version).ThenBy(x => x.Group).ThenBy(x => x.Title).ThenBy(x => x.Player).ToList();
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
    }






    private void Button_PlayCutscene_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            if (button.DataContext is Cutscene cutscene)
            {
                MainWindow.Current.SetFullWindowContent(new CutsceneViewer(cutscene));
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





}






