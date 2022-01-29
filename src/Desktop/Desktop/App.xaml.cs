using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Http;
using Microsoft.UI.Xaml;
using Serilog;
using Serilog.Events;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Windows.Storage;
using Xunkong.Core.Hoyolab;
using Xunkong.Core.Wish;
using Xunkong.Core.XunkongApi;

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
            UnhandledException += App_UnhandledException;
            WeakReferenceMessenger.Default.Register<ConfigureServiceMessage>(this, (_, m) => m.Reply(Task.Run(() => Services = ConfigureServices())));
        }


        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            var logger = Services?.GetService<ILogger<App>>();
            if (logger is not null)
            {
                logger.LogCritical(e.Exception, "App crash.");
            }
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            //var param = AppInstance.GetCurrent().GetActivatedEventArgs();
            if (CheckUserDataPath())
            {
                Services = ConfigureServices();
                m_window = new MainWindow();
                m_window.Activate();
            }
            else
            {
                m_window = new MainWindow(true);
                m_window.Activate();
            }
        }



        private MainWindow m_window;


        public static new App Current => (App)Application.Current;


        public IServiceProvider Services { get; private set; }

        private const string logTemplate = " {NewLine}[{Timestamp:HH:mm:ss.fff}] [{Level:u3}] {SourceContext}{NewLine}{Message}{NewLine}{Exception}";


        private static IServiceProvider ConfigureServices()
        {
#if !DEBUG
            AppCenter.Start("", typeof(Analytics), typeof(Crashes));
#endif
            var setting = ApplicationData.Current.LocalSettings.Values;

            var userDataPath = setting[SettingKeys.UserDataPath] as string;
            var myLogPath = Path.Combine(userDataPath!, $@"Log\Log\log_{DateTime.Now:yyyyMMdd}_{DateTime.Now:HHmmss}.txt");
            var fxLogPath = Path.Combine(userDataPath!, $@"Log\Trace\trace_{DateTime.Now:yyyyMMdd}_{DateTime.Now:HHmmss}.txt");
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
            sc.AddHttpClient<WishlogClient>()
              .ConfigureHttpClient(client => client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br"))
              .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.All });
            sc.AddHttpClient<XunkongApiClient>()
              .ConfigureHttpClient(client =>
              {
                  client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                  client.DefaultRequestHeaders.Add("User-Agent", $"XunkongDesktop/{XunkongEnvironment.AppVersion}");
                  client.DefaultRequestHeaders.Add("X-Platform", "Desktop");
                  client.DefaultRequestHeaders.Add("X-Channel", XunkongEnvironment.Channel.ToString());
                  client.DefaultRequestHeaders.Add("X-Version", XunkongEnvironment.AppVersion.ToString());
                  client.DefaultRequestHeaders.Add("X-Device-Id", XunkongEnvironment.DeviceId);
              })
              .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.All });

            var dbPath = Path.Combine(userDataPath!, @"Data\XunkongData.db");
            var sqlConStr = $"Data Source={dbPath};";
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
            sc.AddPooledDbContextFactory<XunkongDbContext>(options => options.UseSqlite(sqlConStr));
            sc.AddTransient(_ => new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            sc.AddTransient(_ => new DbConnectionFactory<SqliteConnection>(sqlConStr));
            _ = new SqliteConnection(sqlConStr);
            var allTypes = typeof(App).Assembly.GetTypes();
            var serviceTypes = allTypes.Where(x => x.GetCustomAttributes(typeof(InjectServiceAttribute), false).Any());
            foreach (var type in serviceTypes)
            {
                var attrs = type.GetCustomAttributes(typeof(InjectServiceAttribute), false).FirstOrDefault();
                if (attrs is InjectServiceAttribute attr)
                {
                    if (attr.IsSingleton)
                    {
                        sc.AddSingleton(type);
                    }
                    else
                    {
                        sc.AddTransient(type);
                    }
                }
            }
            sc.AddLogging(logBuilder =>
            {
                logBuilder.AddSerilog(fxLogger, true);
                logBuilder.AddSerilog(myLogger, true);
            });
            var service = sc.BuildServiceProvider();


#warning 最好能够删除
            Task.Run(() => service.GetService<IDbContextFactory<XunkongDbContext>>()!.CreateDbContext());

            var _logger = service.GetRequiredService<ILogger<App>>();
            _logger.LogInformation(XunkongEnvironment.GetLogHeader());



            if (File.Exists(dbPath))
            {
                var sb = new StringBuilder();
                sb.AppendLine("XunkongData.db is existed, the followings are applied migraions:");
                var allMigrations = allTypes.Where(x => x.Namespace == "Xunkong.Desktop.Migrations")
                                            .Select(x => (x.GetCustomAttributes(typeof(MigrationAttribute), false).FirstOrDefault() as MigrationAttribute)?.Id)
                                            .Where(x => !string.IsNullOrWhiteSpace(x))
                                            .Distinct()
                                            .ToList();
                var con = new SqliteConnection(sqlConStr);
                var appliedMigrations = con.Query<string>("SELECT * FROM __EFMigrationsHistory;");
                sb.AppendLine(string.Join("\n", appliedMigrations));
                var appendingMigrations = allMigrations.Except(appliedMigrations);
                if (appendingMigrations.Any())
                {
                    sb.AppendLine("The followings are pending migrations:");
                    sb.AppendLine(string.Join("\n", appendingMigrations));
                    var f = service.GetService<IDbContextFactory<XunkongDbContext>>();
                    if (f is not null)
                    {
                        using var context = f.CreateDbContext();
                        sb.AppendLine("Start migration.");
                        _logger.LogInformation(sb.ToString());
                        context.Database.Migrate();
                        _logger.LogInformation("Migration finished");
                    }
                }
                else
                {
                    sb.AppendLine("Database is update to date.");
                    _logger.LogInformation(sb.ToString());
                }
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
