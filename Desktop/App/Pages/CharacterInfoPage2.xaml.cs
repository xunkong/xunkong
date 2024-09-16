// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls;
using Xunkong.Hoyolab.Account;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class CharacterInfoPage2 : Page
{



    private readonly HoyolabService _hoyolabService;

    private static JsonSerializerOptions JsonSerializerOptions = new() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };



    public CharacterInfoPage2()
    {
        this.InitializeComponent();
        _hoyolabService = ServiceProvider.GetService<HoyolabService>()!;
        Loaded += async (_, _) => { await Task.Delay(30); LoadCharacterInfos(); };
        Unloaded += (_, _) => WeakReferenceMessenger.Default.Unregister<SelectedGameRoleChangedMessage>(this);
        WeakReferenceMessenger.Default.Register<SelectedGameRoleChangedMessage>(this, (_, m) =>
        {
            LoadCharacterInfos(m.Role);
        });
    }



    [ObservableProperty]
    private List<CharacterInfoPage2_CharacterInfo> characterInfos;


    [ObservableProperty]
    private string uidAndTime;



    private void LoadCharacterInfos(GenshinRoleInfo? role = null)
    {
        try
        {
            if (role is null)
            {
                role = _hoyolabService.GetLastSelectedOrFirstGenshinRoleInfo();
            }
            if (role != null)
            {
                using var dapper = DatabaseProvider.CreateConnection();
                var time = dapper.QueryFirstOrDefault<DateTimeOffset>("SELECT Time FROM CharacterInfo WHERE Uid=@Uid ORDER BY Time DESC;", new { role.Uid });
                UidAndTime = $"Uid {role.Uid}    Update at {time.LocalDateTime:yyyy-MM-dd HH:mm:ss}";
                var values = dapper.Query<string>("SELECT Value FROM CharacterInfo WHERE Uid=@Uid", new { role.Uid }).ToList();
                var list = values?.Select(x => JsonSerializer.Deserialize<CharacterInfoPage2_CharacterInfo>(x)).ToList();
                if (list != null)
                {
                    CharacterInfos = list.OrderBy(x => x!.Index).ToList()!;
                }
            }
            else
            {
                NotificationProvider.Warning("没有添加账号");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
    }



    [RelayCommand]
    private async Task GetCharacterInfosAsync()
    {
        try
        {
            var role = _hoyolabService.GetLastSelectedOrFirstGenshinRoleInfo();
            if (role != null)
            {
                OperationHistory.AddToDatabase("GetCharacterInfo", role.Uid.ToString());
                Logger.TrackEvent("GetCharacterInfo");
                var list = await _hoyolabService.GetCharacterInfosAsync(role);
                UidAndTime = $"Uid {role.Uid}    Update at {DateTimeOffset.Now.LocalDateTime:yyyy-MM-dd HH:mm:ss}";
                CharacterInfos = list;
                var objs = list.Select(x => new { role.Uid, x.Id, Value = JsonSerializer.Serialize(x, JsonSerializerOptions), Time = DateTimeOffset.Now }).ToList();
                using var dapper = DatabaseProvider.CreateConnection();
                using var t = dapper.BeginTransaction();
                dapper.Execute("INSERT OR REPLACE INTO CharacterInfo (Uid, Id, Value, Time) VALUES (@Uid, @Id, @Value, @Time);", objs, t);
                t.Commit();
                NotificationProvider.Success("已完成");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
    }





}



