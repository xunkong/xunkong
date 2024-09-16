using System.Collections.ObjectModel;

namespace Xunkong.Desktop.ViewModels;

internal partial class AlbumViewModel : ObservableObject
{




    private ObservableCollection<AlbumPage_ImageFoler> _FolderList = new();
    public ObservableCollection<AlbumPage_ImageFoler> FolderList
    {
        get => _FolderList;
        set => SetProperty(ref _FolderList, value);
    }




    public void InitializeImageFolder()
    {
        try
        {
            if (!FolderList.Any(x => x.Header == "游戏截图"))
            {
                var folder = GetScreenshotFolder();
                if (folder != null)
                {
                    FolderList.Add(new AlbumPage_ImageFoler("游戏截图", folder));
                }
            }
            if (!FolderList.Any(x => x.Header == "截图备份"))
            {
                var folder = AppSetting.GetValue<string>(SettingKeys.GameScreenshotBackupFolder) ?? Path.Combine(XunkongEnvironment.UserDataPath, "Screenshot");
                if (Directory.Exists(folder))
                {
                    FolderList.Add(new AlbumPage_ImageFoler("截图备份", folder));
                }
            }
            if (!FolderList.Any(x => x.Header == "壁纸收藏"))
            {
                var folder = AppSetting.GetValue<string>(SettingKeys.WallpaperSaveFolder) ?? Path.Combine(XunkongEnvironment.UserDataPath, "Wallpaper");
                if (Directory.Exists(folder))
                {
                    FolderList.Add(new AlbumPage_ImageFoler("壁纸收藏", folder));
                }
            }
        }
        catch (Exception ex)
        {
            NotificationProvider.Error(ex, "初始化相册页面");
            Logger.Error(ex, "初始化相册页面");
        }
    }





    private static string? GetScreenshotFolder()
    {
        var folder = AppSetting.GetValue<string>(SettingKeys.ScreenFolderPath);
        if (Directory.Exists(folder))
        {
            return folder;
        }
        var exe = GameAccountService.GetGameExePath(0);
        folder = Path.Join(Path.GetDirectoryName(exe), "ScreenShot");
        if (Directory.Exists(folder))
        {
            AppSetting.SetValue(SettingKeys.ScreenFolderPath, folder);
            return folder;
        }
        exe = GameAccountService.GetGameExePath(1);
        folder = Path.Join(Path.GetDirectoryName(exe), "ScreenShot");
        if (Directory.Exists(folder))
        {
            AppSetting.SetValue(SettingKeys.ScreenFolderPath, folder);
            return folder;
        }
        folder = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "GenshinImpactCloudGame");
        if (Directory.Exists(folder))
        {
            AppSetting.SetValue(SettingKeys.ScreenFolderPath, folder);
            return folder;
        }
        return null;
    }





}
