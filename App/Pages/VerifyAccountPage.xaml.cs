// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Vanara.PInvoke;
using Windows.ApplicationModel.Activation;
using Xunkong.Desktop.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class VerifyAccountPage : Page
{

    private readonly HoyolabService hoyolabService = new HoyolabService(new Hoyolab.HoyolabClient());

    public VerifyAccountPage()
    {
        this.InitializeComponent();
    }

    public VerifyAccountPage(IToastNotificationActivatedEventArgs args)
    {
        _args = args;
        this.InitializeComponent();
        Loaded += VerifyAccountPage_Loaded;
        ToolWindow.Current.ResizeToCenter(440, 660);
        int.TryParse(_args.Argument.Replace("DailyNoteTask_VerifyAccount_", ""), out uid);
    }

    private IToastNotificationActivatedEventArgs _args;

    int uid = 0;

    private void VerifyAccountPage_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var role = hoyolabService.GetGenshinRoleInfo(uid);
            if (role is not null)
            {
                var accts = hoyolabService.GetHoyolabUserInfoList().ToList();
                foreach (var item in accts)
                {
                    if (role.Cookie == item.Cookie)
                    {
                        webBridge.Content = new DailyNoteWebBridge(item, role);
                        User32.SetForegroundWindow(ToolWindow.Current.HWND);
                        return;
                    }
                }
            }
            c_StackPanel_Error.Visibility = Visibility.Visible;
            TextBlock_Tip.Visibility = Visibility.Collapsed;
            Button_Finish.Visibility = Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            TextBlock_Tip.Visibility = Visibility.Collapsed;
            Button_Finish.Visibility = Visibility.Collapsed;
            TextBlock_Error.Text = ex.Message;
        }
    }


    private async void Button_Finish_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var role = hoyolabService.GetGenshinRoleInfo(uid);
            if (role != null)
            {
                var note = await hoyolabService.GetDailyNoteAsync(role, true);
                if (note != null)
                {
                    SecondaryTileProvider.UpdatePinnedTile(note);
                    Button_Finish.Content = "磁贴已更新";
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            Button_Finish.Content = ex.Message;
        }
    }

}
