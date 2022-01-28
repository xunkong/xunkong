using AngleSharp;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Xunkong.Core.XunkongApi;
using Xunkong.Desktop.Services;

namespace Xunkong.Desktop.ViewModels
{

    [InjectService]
    internal partial class SettingViewModel : ObservableObject
    {

        private readonly ILogger<SettingViewModel> _logger;

        private readonly IDbContextFactory<XunkongDbContext> _dbContextFactory;

        private readonly DbConnectionFactory<SqliteConnection> _dbConnectionFactory;

        private readonly XunkongDbContext _webToolItemDbContext;

        private readonly HttpClient _httpClient;

        private readonly XunkongApiService _xunkongApiService;

        private readonly WishlogService _wishlogService;

        private readonly HoyolabService _hoyolabService;



        public string AppName => XunkongEnvironment.AppName;

        public string AppVersion => XunkongEnvironment.AppVersion.ToString();




        public SettingViewModel(ILogger<SettingViewModel> logger,
                                IDbContextFactory<XunkongDbContext> dbContextFactory,
                                DbConnectionFactory<SqliteConnection> dbConnectionFactory,
                                HttpClient httpClient,
                                XunkongApiService xunkongApiService,
                                WishlogService wishlogService,
                                HoyolabService hoyolabService)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _dbConnectionFactory = dbConnectionFactory;
            _webToolItemDbContext = dbContextFactory.CreateDbContext();
            _httpClient = httpClient;
            _xunkongApiService = xunkongApiService;
            _wishlogService = wishlogService;
            _hoyolabService = hoyolabService;
        }




        #region WebTool Setting



        private ObservableCollection<WebToolItem> _WebToolItemList;
        public ObservableCollection<WebToolItem> WebToolItemList
        {
            get => _WebToolItemList;
            set => SetProperty(ref _WebToolItemList, value);
        }


        private WebToolItem? _SelectedWebToolItem;
        public WebToolItem? SelectedWebToolItem
        {
            get => _SelectedWebToolItem;
            set => SetProperty(ref _SelectedWebToolItem, value);
        }



        public async Task InitializeDataAsync()
        {
            try
            {
                var list = await _webToolItemDbContext.WebToolItems.OrderBy(x => x.Order).ToListAsync();
                _WebToolItemList = new(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when init webtool setting items.");
                InfoBarHelper.Error(ex, "无法加载网页小工具的数据");
            }
        }


        private ChannelType _SelectedChannel = XunkongEnvironment.Channel;
        public ChannelType SelectedChannel
        {
            get => _SelectedChannel;
            set => SetProperty(ref _SelectedChannel, value);
        }


        public List<ChannelType> ChannelTypeList { get; set; } = Enum.GetValues<ChannelType>().ToList();



        [ICommand(AllowConcurrentExecutions = false)]
        private async Task CheckUpdateAsync()
        {
            try
            {
                var version = await _xunkongApiService.CheckUpdateAsync(SelectedChannel);
                if (version.Version > XunkongEnvironment.AppVersion && !string.IsNullOrWhiteSpace(version.PackageUrl))
                {
                    var button = new Button { Content = "下载", HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Right };
                    button.Click += (_, _) => Process.Start(new ProcessStartInfo
                    {
                        FileName = version.PackageUrl,
                        UseShellExecute = true,
                    });
                    var infoBar = new InfoBar
                    {
                        Severity = InfoBarSeverity.Success,
                        Title = $"新版本 {version.Version}",
                        Message = version.Abstract,
                        ActionButton = button,
                        IsOpen = true,
                    };
                    InfoBarHelper.Show(infoBar);
                }
                else
                {
                    InfoBarHelper.Information("已是最新版本");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {MethodName}", nameof(CheckUpdateAsync));
                InfoBarHelper.Error(ex);
            }
        }


        [ICommand]
        private void AddWebToolItem()
        {
            var newItem = new WebToolItem();
            if (WebToolItemList is null)
            {
                WebToolItemList = new();
            }
            WebToolItemList.Add(newItem);
            SelectedWebToolItem = newItem;
        }



        [ICommand]
        private void DeleteSelectedWebToolItem()
        {
            try
            {
                if (SelectedWebToolItem is not null)
                {
                    if (SelectedWebToolItem.Id != 0)
                    {
                        _logger.LogDebug("Deleting WebToolItem {Title}", SelectedWebToolItem.Title);
                        _webToolItemDbContext.Remove(SelectedWebToolItem);
                    }
                    var index = WebToolItemList.IndexOf(SelectedWebToolItem);
                    WebToolItemList.Remove(SelectedWebToolItem);
                    if (WebToolItemList.Count < index + 1)
                    {
                        SelectedWebToolItem = WebToolItemList.LastOrDefault();
                    }
                    else
                    {
                        SelectedWebToolItem = WebToolItemList.ElementAt(index);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {MethodName}", nameof(DeleteSelectedWebToolItem));
                InfoBarHelper.Error(ex);
            }

        }



        [ICommand]
        private void CloseEditWebToolGrid()
        {
            SelectedWebToolItem = null;
        }



        [ICommand]
        private async Task GetTitleAndIconByUrlAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedWebToolItem?.Url))
            {
                return;
            }
            var url = SelectedWebToolItem.Url;
            _logger.LogDebug("Get WebToolItem title and icon by url {Url}", url);
            try
            {
                var uri = new Uri(url);
                var baseUri = new Uri($"{uri.Scheme}://{uri.Host}");
                var html = await _httpClient.GetStringAsync(url);
                var context = BrowsingContext.New(Configuration.Default);
                var dom = await context.OpenAsync(x => { x.Content(html); x.Address(url); });
                SelectedWebToolItem.Title = dom.Title;
                var iconList = dom.Head?.Children.Where(x => x.Attributes["rel"]?.Value.Contains("icon") ?? false);
                var href = iconList?.FirstOrDefault()?.Attributes["href"]?.Value;
                if (string.IsNullOrWhiteSpace(href))
                {
                    SelectedWebToolItem.Icon = new Uri(baseUri, "favicon.ico").ToString();
                }
                else
                {
                    SelectedWebToolItem.Icon = new Uri(baseUri, href).ToString();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {MethodName}", nameof(GetTitleAndIconByUrlAsync));
                InfoBarHelper.Error(ex);
            }
        }





        [ICommand]
        private async Task SaveWebToolItemAsync()
        {
            try
            {
                var list = WebToolItemList.Where(x => !string.IsNullOrWhiteSpace(x.Url)).ToList();
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].Order = i;
                }
                WebToolItemList = new(list);
                _webToolItemDbContext.UpdateRange(list);
                await _webToolItemDbContext.SaveChangesAsync();
                _logger.LogDebug("Saved the above WebToolItem changes.");
                InfoBarHelper.Success("保存成功");
                WeakReferenceMessenger.Default.Send<RefreshWebToolNavItemMessage>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {MethodName}", nameof(SaveWebToolItemAsync));
                InfoBarHelper.Error(ex);
            }
        }



        #endregion




        [ICommand]
        private async Task RegisterBackgroundTask()
        {
            try
            {
                var a = BackgroundTaskRegistration.AllTasks;
                foreach (var item in a)
                {
                    if (item.Value.Name == "My Background Trigger")
                    {
                        item.Value.Unregister(true);
                    }
                }
                var requestStatus = await BackgroundExecutionManager.RequestAccessAsync();
                var builder = new BackgroundTaskBuilder();
                builder.Name = "My Background Trigger";
                builder.SetTrigger(new TimeTrigger(15, false));
                //builder.TaskEntryPoint = typeof(DailyNoteTask).FullName;
                builder.SetTaskEntryPointClsid(Guid.NewGuid());
                // Do not set builder.TaskEntryPoint for in-process background tasks
                // Here we register the task and work will start based on the time trigger.
                BackgroundTaskRegistration task = builder.Register();
                await Task.Delay(3000);
            }
            catch (Exception ex)
            {

                throw;
            }

        }






        [ICommand(AllowConcurrentExecutions = false)]
        private async Task GetTravelRecordSummary()
        {
            try
            {
                var role = await _hoyolabService.GetLastSelectedOrFirstUserGameRoleInfoAsync();
                var now = DateTime.UtcNow.AddHours(8);
                var month = now.Month;
                var r = await _hoyolabService.GetTravelRecordSummaryAsync(role, month);
                var s = await _hoyolabService.GetTravelRecordDetailAsync(role, month, Core.TravelRecord.TravelRecordAwardType.Primogems);
                var t = await _hoyolabService.GetTravelRecordDetailAsync(role, month, Core.TravelRecord.TravelRecordAwardType.Mora);
                month = now.AddMonths(-1).Month;
                r = await _hoyolabService.GetTravelRecordSummaryAsync(role, month);
                s = await _hoyolabService.GetTravelRecordDetailAsync(role, month, Core.TravelRecord.TravelRecordAwardType.Primogems);
                t = await _hoyolabService.GetTravelRecordDetailAsync(role, month, Core.TravelRecord.TravelRecordAwardType.Mora);
                month = now.AddMonths(-2).Month;
                r = await _hoyolabService.GetTravelRecordSummaryAsync(role, month);
                s = await _hoyolabService.GetTravelRecordDetailAsync(role, month, Core.TravelRecord.TravelRecordAwardType.Primogems);
                t = await _hoyolabService.GetTravelRecordDetailAsync(role, month, Core.TravelRecord.TravelRecordAwardType.Mora);
                InfoBarHelper.Success("成功");
            }
            catch (Exception ex)
            {
                InfoBarHelper.Error(ex);
                throw;
            }
        }


        [ICommand(AllowConcurrentExecutions = false)]
        private async Task GetSpiralAbyssInfoSummary()
        {
            try
            {
                var role = await _hoyolabService.GetLastSelectedOrFirstUserGameRoleInfoAsync();
                var c = await _hoyolabService.GetSpiralAbyssInfoAsync(role, 1);
                var l = await _hoyolabService.GetSpiralAbyssInfoAsync(role, 2);
                InfoBarHelper.Success("成功");
            }
            catch (Exception ex)
            {
                InfoBarHelper.Error(ex);
                throw;
            }
        }



    }
}
