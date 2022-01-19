using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Xunkong.Core.Hoyolab;
using Xunkong.Desktop.Services;
using System.Net;

namespace Xunkong.Desktop.ViewModels
{
    internal partial class MainWindowViewModel : ObservableObject
    {



        private readonly IDbContextFactory<XunkongDbContext> _dbFactory;

        private readonly UserSettingService _userSettingService;

        private readonly HoyolabService _hoyolabService;


        public MainWindowViewModel() { }



        public MainWindowViewModel(IDbContextFactory<XunkongDbContext> dbFactory, UserSettingService userSettingService, HoyolabService hoyolabService)
        {
            _dbFactory = dbFactory;
            _userSettingService = userSettingService;
            _hoyolabService = hoyolabService;
        }








    }
}
