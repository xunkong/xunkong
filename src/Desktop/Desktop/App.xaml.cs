global using Dapper;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Xunkong.Desktop.Controls;
global using Xunkong.Desktop.Helpers;
global using Xunkong.Desktop.Models;
global using Xunkong.Desktop.Services;
global using Xunkong.Desktop.ViewModels;
global using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http;
using Microsoft.UI.Xaml;
using Serilog;
using Serilog.Events;
using System.Text;
using Windows.Storage;
using Xunkong.Core.Hoyolab;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (CheckUserDataPath())
            {
                Services = ConfigureServices();
                m_window = new MainWindow();
                m_window.Activate();
            }
            else
            {
                m_window = new MainWindow(true);
                m_window.ConfigureServices = ConfigureServices;
                m_window.Activate();
            }
        }


        private MainWindow m_window;


        public static new App Current => (App)Application.Current;


        public IServiceProvider Services { get; set; }

        private const string logTemplate = "{NewLine}[{Timestamp:HH:mm:ss.fff}] [{Level:u3}] {SourceContext}{NewLine}{Message}{NewLine}{Exception}";


        private static IServiceProvider ConfigureServices()
        {
            var setting = ApplicationData.Current.LocalSettings.Values;

            var userDataPath = setting[SettingKeys.UserDataPath] as string;
            var myLogPath = Path.Combine(userDataPath!, $@"Log\Log\XunkongLog_{DateTime.Now:yyyyMMdd}_{DateTime.Now:HHmmss}.txt");
            var fxLogPath = Path.Combine(userDataPath!, $@"Log\Trace\XunkongTrace_{DateTime.Now:yyyyMMdd}_{DateTime.Now:HHmmss}.txt");
            var fxLogger = new LoggerConfiguration().MinimumLevel.Verbose()
                                                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                                                    .WriteTo.Async(x => x.File(path: fxLogPath, outputTemplate: logTemplate, shared: true, retainedFileCountLimit: 1000))
                                                    .Enrich.FromLogContext()
                                                    .CreateLogger();
            var myLogger = new LoggerConfiguration().MinimumLevel.Verbose()
                                                    .Filter.ByIncludingOnly("StartsWith(SourceContext, 'Xunkong')")
                                                    .WriteTo.Async(x => x.File(path: myLogPath, outputTemplate: logTemplate, shared: true, retainedFileCountLimit: 1000))
                                                    .Enrich.FromLogContext()
                                                    .CreateLogger();

            var sc = new ServiceCollection();
            sc.Configure<HttpClientFactoryOptions>(options =>
            {
                options.HttpClientActions.Add(client => client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br"));
                options.HttpMessageHandlerBuilderActions.Add(builder =>
                    builder.PrimaryHandler = new HttpClientHandler
                    {
                        AutomaticDecompression = System.Net.DecompressionMethods.All,
                    });
            });
            sc.AddHttpClient<HoyolabClient>()
              .ConfigureHttpClient(client => client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br"))
              .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.All });

            var dbPath = Path.Combine(userDataPath!, @"Data\XunkongData.db");
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
            sc.AddPooledDbContextFactory<XunkongDbContext>(options => options.UseSqlite($"Data Source={dbPath};"));
            sc.AddLogging(logBuilder =>
            {
                logBuilder.AddSerilog(fxLogger, true);
                logBuilder.AddSerilog(myLogger, true);
            });
            sc.AddTransient(_ => new DbConnectionFactory<SqliteConnection>($"Data Source={dbPath};"));
            sc.AddTransient<UserSettingService>();
            sc.AddTransient<HoyolabService>();
            sc.AddTransient<MainWindowViewModel>();
            sc.AddTransient<UserPanelViewModel>();
            var service = sc.BuildServiceProvider();

            var _logger = service.GetRequiredService<ILogger<App>>();
            _logger.LogInformation(XunkongEnvironment.GetLogHeader());

            if (File.Exists(dbPath))
            {
                Task.Run(async () =>
                {
                    var f = service.GetService<IDbContextFactory<XunkongDbContext>>();
                    if (f is not null)
                    {
                        using var context = f.CreateDbContext();
                        var ams = await context.Database.GetAppliedMigrationsAsync();
                        var pms = await context.Database.GetPendingMigrationsAsync();
                        var sb = new StringBuilder();
                        sb.AppendLine("XunkongData.db is existed, the followings are applied migraions:");
                        sb.AppendLine(string.Join("\n", ams));
                        if (pms.Any())
                        {
                            sb.AppendLine("The followings are pending migrations:");
                            sb.AppendLine(string.Join("\n", pms));
                            sb.AppendLine("Start migration.");
                            _logger.LogInformation(sb.ToString());
                            await context.Database.MigrateAsync();
                            _logger.LogInformation("Migration finished");
                        }
                        else
                        {
                            sb.AppendLine("Database is update to date.");
                            _logger.LogInformation(sb.ToString());
                        }
                    }
                });
            }
            else
            {
                _logger.LogInformation("XunkongData.db is not existed, start migrate database.");
                var f = service.GetService<IDbContextFactory<XunkongDbContext>>();
                if (f is not null)
                {
                    using var context = f.CreateDbContext();
                    context.Database.Migrate();
                    _logger.LogInformation("Migration finished");
                }
            }
            return service;
        }




        private bool CheckUserDataPath()
        {
            var userDataPath = ApplicationData.Current.LocalSettings.Values[SettingKeys.UserDataPath] as string;
            if (string.IsNullOrWhiteSpace(userDataPath))
            {
                return false;
            }
            if (File.Exists(Path.Combine(userDataPath, "XunkongRoot")))
            {
                return true;
            }
            else
            {
                return false;
            }
        }





    }
}
