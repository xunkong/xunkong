// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using AngleSharp;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Net.Http;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[INotifyPropertyChanged]
public sealed partial class WebToolEditPage : Page
{


    private readonly HttpClient _httpClient;



    public WebToolEditPage()
    {
        this.InitializeComponent();
        _httpClient = ServiceProvider.GetService<HttpClient>()!;
        Loaded += (_, _) => LoadWebToolItems();
    }



    #region WebTool


    /// <summary>
    /// 快捷方式列表
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<WebToolItem> _WebToolItemList;

    /// <summary>
    /// 选中的快捷方式
    /// </summary>
    [ObservableProperty]
    private WebToolItem? _SelectedWebToolItem;



    /// <summary>
    /// 加载网页快捷方式
    /// </summary>
    public void LoadWebToolItems()
    {
        try
        {
            using var dapper = DatabaseProvider.CreateConnection();
            var list = dapper.Query<WebToolItem>("SELECT * FROM WebToolItem ORDER BY [Order];");
            WebToolItemList = new(list);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "无法加载网页快捷方式的数据");
            NotificationProvider.Error(ex, "无法加载网页快捷方式的数据");
        }
    }


    /// <summary>
    /// 添加新的网页快捷方式
    /// </summary>
    [RelayCommand]
    private void AddWebToolItem()
    {
        try
        {
            var newItem = new WebToolItem();
            if (WebToolItemList is null)
            {
                WebToolItemList = new();
            }
            WebToolItemList.Add(newItem);
            SelectedWebToolItem = newItem;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "添加新的网页快捷方式");
        }

    }


    /// <summary>
    /// 删除选择的网页快捷方式，但未保存至数据库
    /// </summary>
    [RelayCommand]
    private void DeleteSelectedWebToolItem()
    {
        try
        {
            if (SelectedWebToolItem is not null)
            {
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
            NotificationProvider.Error(ex, "删除选择的网页快捷方式");
        }

    }


    /// <summary>
    /// 关闭网页快捷方式编辑栏
    /// </summary>
    [RelayCommand]
    private void CloseEditWebToolGrid()
    {
        SelectedWebToolItem = null;
    }


    /// <summary>
    /// 获取网页的图标
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task GetTitleAndIconByUrlAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedWebToolItem?.Url))
        {
            return;
        }
        var url = SelectedWebToolItem.Url;
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
            Logger.Error(ex, "获取网页的图标");
            NotificationProvider.Error(ex, "获取网页的图标");
        }
    }




    /// <summary>
    /// 保存网页快捷方式的修改
    /// </summary>
    [RelayCommand]
    private void SaveWebToolItem()
    {
        try
        {
            var list = WebToolItemList.Where(x => !string.IsNullOrWhiteSpace(x.Url)).ToList();
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Order = i;
            }
            using var dapper = DatabaseProvider.CreateConnection();
            using var t = dapper.BeginTransaction();
            dapper.Execute("DELETE FROM WebToolItem WHERE TRUE;", t);
            dapper.Execute("INSERT INTO WebToolItem (Title, Icon, [Order], Url, JavaScript) VALUES (@Title, @Icon, @Order, @Url, @JavaScript);", list, t);
            t.Commit();
            WebToolItemList = new(list);
            NotificationProvider.Success("保存成功");
            MainPage.Current.InitializeNavigationWebToolItem();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "保存网页快捷方式");
            NotificationProvider.Error(ex, "保存网页快捷方式");
        }
    }



    #endregion













}
