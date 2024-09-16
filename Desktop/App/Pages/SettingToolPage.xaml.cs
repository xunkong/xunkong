// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.Activation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SettingToolPage : Page
{


    public SettingToolPage()
    {
        this.InitializeComponent();
        ToolWindow.Current.ResizeToCenter(360, 480);
    }



    public SettingToolPage(IProtocolActivatedEventArgs args)
    {
        this.InitializeComponent();
        ToolWindow.Current.ResizeToCenter(360, 480);
    }

    private void Button_Save_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var key = TextBox_Key.Text;
            if (string.IsNullOrWhiteSpace(key))
            {
                TextBlock_State.Text = "键为空";
                return;
            }
            var text = TextBox_Value.Text;
            if (bool.TryParse(text, out var b))
            {
                AppSetting.SetValue(key, b);
                TextBlock_State.Text = "成功";
                return;
            }
            if (int.TryParse(text, out var i))
            {
                AppSetting.SetValue(key, i);
                TextBlock_State.Text = "成功";
                return;
            }
            AppSetting.SetValue(key, text);
            TextBlock_State.Text = "成功";
        }
        catch (Exception ex)
        {
            TextBlock_State.Text = "失败 - " + ex.Message;
        }
    }
}
