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

    [InjectService]
    internal partial class WindowRootViewModel : ObservableObject
    {

        private readonly ILogger<WindowRootViewModel> _logger;

        private readonly IDbContextFactory<XunkongDbContext> _dbFactory;

        private readonly UserSettingService _userSettingService;

        private readonly HoyolabService _hoyolabService;

        private readonly XunkongApiService _xunkongApiService;



        public WindowRootViewModel(ILogger<WindowRootViewModel> logger, IDbContextFactory<XunkongDbContext> dbFactory, UserSettingService userSettingService, HoyolabService hoyolabService, XunkongApiService xunkongApiService)
        {
            _logger = logger;
            _dbFactory = dbFactory;
            _userSettingService = userSettingService;
            _hoyolabService = hoyolabService;
            _xunkongApiService = xunkongApiService;
        }







    }
}
