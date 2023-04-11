// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Xunkong.Hoyolab.Account;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class DailyNoteSettingPage : Page
{


    public class GenshinRoleInfoEx : GenshinRoleInfo
    {

        public int Sort { get; set; }


        public bool DisableDailyNote { get; set; }

    }


    public DailyNoteSettingPage()
    {
        this.InitializeComponent();
        Loaded += DailyNoteSettingPage_Loaded;
    }


    [ObservableProperty]
    private ObservableCollection<GenshinRoleInfoEx> roles;


    private void DailyNoteSettingPage_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            using var dapper = DatabaseProvider.CreateConnection();
            var r = dapper.Query<GenshinRoleInfoEx>("SELECT * FROM GenshinRoleInfo ORDER BY Sort DESC;").ToList();
            foreach (var item in r)
            {
                item.DisableDailyNote = !item.DisableDailyNote;
            }
            Roles = new(r);
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }

    }


    [RelayCommand]
    private void Save()
    {
        try
        {
            if (Roles?.Any() ?? false)
            {
                var list = new List<DataModel>();
                var count = Roles.Count;
                for (int i = 0; i < count; i++)
                {
                    var role = Roles[i];
                    list.Add(new DataModel(role.Uid, count - i, !role.DisableDailyNote));
                }
                using var dapper = DatabaseProvider.CreateConnection();
                using var t = dapper.BeginTransaction();
                dapper.Execute("UPDATE GenshinRoleInfo SET Sort=@Sort, DisableDailyNote=@DisableDailyNote WHERE Uid=@Uid;", list);
                t.Commit();
                NotificationProvider.Success("保存成功");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
    }


    private void ToggleSwitch_EnableDailyNote_Toggled(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is FrameworkElement fe)
            {
                if (fe.DataContext is GenshinRoleInfoEx role)
                {
                    using var dapper = DatabaseProvider.CreateConnection();
                    dapper.Execute("UPDATE GenshinRoleInfo SET DisableDailyNote=@DisableDailyNote WHERE Uid=@Uid;", new { role.Uid, DisableDailyNote = !role.DisableDailyNote });
                }
            }
        }
        catch { }
    }


    private record DataModel(int Uid, int Sort, bool DisableDailyNote);



}

