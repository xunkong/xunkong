using Microsoft.UI.Xaml.Data;
using System.Collections.ObjectModel;

namespace Xunkong.Desktop.Models;




public class AlbumPage_ImageFoler : ObservableObject, IDisposable
{



    private FileSystemWatcher _watcher;


    public string Header { get; set; }


    public AlbumPage_ImageFoler(string header, string folder)
    {
        Header = header;
        _watcher = new FileSystemWatcher(folder);
        _watcher.Filter = "*";
        _watcher.Created += FileSystemWatcher_Created;
        _watcher.Deleted += FileSystemWatcher_Deleted;
        _watcher.EnableRaisingEvents = true;
        var files = new DirectoryInfo(folder).GetFiles().Where(IsAvaliableExtesion).OrderByDescending(x => x.CreationTime);
        ImageList = new(files.Select(x => new AlbumPage_ImageItem(x)));
        var queryGroup = ImageList.GroupBy(x => x.CreationTime.ToString("yyyy-MM")).Select(x => new AlbumPage_ImageGroupCollection(x.Key, x)).OrderByDescending(x => x.Header);
        ImageGroupList = new(queryGroup);
        ImageSource = new CollectionViewSource { Source = ImageGroupList, IsSourceGrouped = true };
    }





    private ObservableCollection<AlbumPage_ImageItem> _ImageList;
    public ObservableCollection<AlbumPage_ImageItem> ImageList
    {
        get => _ImageList;
        set => SetProperty(ref _ImageList, value);
    }




    private ObservableCollection<AlbumPage_ImageGroupCollection> _ImageGroupList;
    public ObservableCollection<AlbumPage_ImageGroupCollection> ImageGroupList
    {
        get => _ImageGroupList;
        set => SetProperty(ref _ImageGroupList, value);
    }



    private CollectionViewSource _ImageSource;
    public CollectionViewSource ImageSource
    {
        get => _ImageSource;
        set => SetProperty(ref _ImageSource, value);
    }


    private AlbumPage_ImageGroupCollection newImages;




    private void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
    {
        try
        {
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                var fileInfo = new FileInfo(e.FullPath);
                if (fileInfo.Exists && IsAvaliableExtesion(fileInfo))
                {
                    var item = new AlbumPage_ImageItem(fileInfo);
                    var dq = MainWindow.Current.DispatcherQueue;
                    if (newImages is null)
                    {
                        newImages = new("新增", new[] { item });
                        dq.TryEnqueue(() => ImageGroupList?.Insert(0, newImages));
                    }
                    else
                    {
                        dq.TryEnqueue(() => newImages.Add(item));
                    }
                    dq.TryEnqueue(() => ImageList.Insert(newImages.Count - 1, item));
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "截图文件夹内有新文件");
        }
    }



    private void FileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
    {
        try
        {
            if (e.ChangeType == WatcherChangeTypes.Deleted)
            {
                var path = Path.GetFullPath(e.FullPath);
                var dq = MainWindow.Current.DispatcherQueue;
                dq.TryEnqueue(() =>
                {
                    var file1 = newImages?.FirstOrDefault(x => x.FullName == path);
                    if (file1 is not null)
                    {
                        newImages?.Remove(file1);
                    }
                    var file2 = ImageList?.FirstOrDefault(x => x.FullName == path);
                    if (file2 is not null)
                    {
                        ImageList?.Remove(file2);
                    }
                    foreach (var item in ImageGroupList)
                    {
                        var file3 = item.FirstOrDefault(x => x.FullName == path);
                        if (file3 is not null)
                        {
                            item.Remove(file3);
                            break;
                        }
                    }
                });
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "截图文件夹内有文件被删除");
        }
    }



    public static bool IsAvaliableExtesion(FileInfo info)
    {
        return info.Extension is ".avif" or ".bmp" or ".heic" or ".jpg" or ".jpeg" or ".png" or ".webp";
    }


    public void Dispose()
    {
        ((IDisposable)_watcher).Dispose();
    }
}




public class AlbumPage_ImageItem
{

    private static string[] format = new[] { "yyyyMMddHHmmss", "yyyy_MM_dd HH_mm_ss" };

    public AlbumPage_ImageItem(FileInfo info)
    {
        FullName = info.FullName;
        var name = Path.GetFileNameWithoutExtension(info.Name);
        if (name.StartsWith("GenshinlmpactPhoto"))
        {
            name = name["GenshinlmpactPhoto".Length..];
        }
        if (DateTime.TryParseExact(name, format, null, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out var time))
        {
            CreationTime = time;
            Title = time.ToString("yyyy-MM-dd HH:mm:ss");
        }
        else
        {
            Title = name;
            CreationTime = info.CreationTime;
        }
    }

    public string Title { get; set; }

    public string FullName { get; set; }

    public DateTime CreationTime { get; set; }

}



public class AlbumPage_ImageGroupCollection : ObservableCollection<AlbumPage_ImageItem>
{

    public string Header { get; set; }


    public AlbumPage_ImageGroupCollection(string header, IEnumerable<AlbumPage_ImageItem> list) : base(list)
    {
        Header = header;
    }

}

