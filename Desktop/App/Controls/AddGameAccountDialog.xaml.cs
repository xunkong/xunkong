// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Controls;

[INotifyPropertyChanged]
public sealed partial class AddGameAccountDialog : UserControl
{


    public AddGameAccountDialog()
    {
        this.InitializeComponent();
    }


    [ObservableProperty]
    private List<GameAccount> _GameAccounts;


    [ObservableProperty]
    private GameAccount _SelectedAccount;

    partial void OnSelectedAccountChanged(GameAccount value)
    {
        TextBox_Name.Text = value.Name;
    }


    [ObservableProperty]
    private string showText;


    [RelayCommand]
    private void SaveGameAccount()
    {
        try
        {
            ShowText = "";
            if (SelectedAccount == null)
            {
                ShowText = "需要选择一个账号";
                return;
            }
            var name = TextBox_Name.Text?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                ShowText = "需要输入一个看得见的名字";
                return;
            }
            SelectedAccount.Name = name;
            GameAccountService.SaveGameAccount(SelectedAccount);
            ShowText = "保存成功";
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            ShowText = ex.Message;
        }
    }




}
