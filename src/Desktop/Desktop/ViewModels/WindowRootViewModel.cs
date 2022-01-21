using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Xunkong.Core.Hoyolab;
using Xunkong.Desktop.Services;
using System.Net;
using System.Collections.ObjectModel;

namespace Xunkong.Desktop.ViewModels
{
    internal partial class WindowRootViewModel : ObservableObject
    {



        private readonly IDbContextFactory<XunkongDbContext> _dbFactory;

        private readonly UserSettingService _userSettingService;

        private readonly HoyolabService _hoyolabService;




        public WindowRootViewModel(IDbContextFactory<XunkongDbContext> dbFactory, UserSettingService userSettingService, HoyolabService hoyolabService)
        {
            _dbFactory = dbFactory;
            _userSettingService = userSettingService;
            _hoyolabService = hoyolabService;
        }




        private ObservableCollection<WebToolItem> _WebToolItemList;
        public ObservableCollection<WebToolItem> WebToolItemList
        {
            get => _WebToolItemList;
            set => SetProperty(ref _WebToolItemList, value);
        }






    }
}
