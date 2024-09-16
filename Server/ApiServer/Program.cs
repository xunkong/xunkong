using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpLogging;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Xunkong Web Api",
        Version = "v1",
    });
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Xunkong.ApiServer.xml"), true);
});
builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
});
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});


builder.Services.AddResponseCompression();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<WishlogAuthActionFilter>();
builder.Services.AddScoped<BaseRecordResultFilter>();
builder.Services.AddScoped<WishlogRecordFilter>();

builder.Services.AddDbContextPool<XunkongDbContext>(options =>
{
#if DEBUG
    options.UseMySql(builder.Configuration.GetConnectionString("constr_xunkong"), new MySqlServerVersion("8.0.31")).EnableSensitiveDataLogging(true);
#else
    options.UseMySql(Environment.GetEnvironmentVariable("CONSTR"), new MySqlServerVersion("8.0.28"));
#endif
});

#if DEBUG
builder.Services.AddSingleton(new DbConnectionFactory(builder.Configuration.GetConnectionString("constr_xunkong")!));
#else
builder.Services.AddSingleton(new DbConnectionFactory(Environment.GetEnvironmentVariable("CONSTR")!));
#endif

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ExceptionFilter>();
    options.Filters.Add<BaseWrapperFilter>();
});
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.WriteIndented = true;
    options.JsonSerializerOptions.AllowTrailingCommas = true;
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip;
});

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options => options.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fffffff zzz\n");

builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

builder.Services.Configure<ApiBehaviorOptions>(options => options.InvalidModelStateResponseFactory = (context) => throw new XunkongApiServerException(ErrorCode.InvalidModelException));

builder.Services.AddHttpClient<WishlogClient>().ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.All });

builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.RequestProperties;
    //options.LoggingFields = HttpLoggingFields.RequestPropertiesAndHeaders | HttpLoggingFields.ResponsePropertiesAndHeaders;
    //options.RequestHeaders.Add("X-Channel");
    //options.RequestHeaders.Add("X-Device-Id");
    //options.RequestHeaders.Add("X-Platform");
    //options.RequestHeaders.Add("X-Version");
    //options.RequestHeaders.Add("X-Forwarded-For");
    //options.RequestHeaders.Add("X-Forwarded-Proto");
});



var app = builder.Build();

// 异常捕获
app.UseExceptionHandler(c => c.Run(async context =>
{
    var feature = context.Features.Get<IExceptionHandlerPathFeature>();
    var ex = feature?.Error;
    app.Services.GetService<ILogger>()?.LogError(ex, "Exception middleware");
    var result = new { Code = ErrorCode.InternalException, ex?.Message };
    context.Response.StatusCode = 500;
    await context.Response.WriteAsJsonAsync(result);
}));


app.UseResponseCompression();

app.UseHttpLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Xunkong Web Api v1");
    });
}

app.UseAuthorization();

app.MapControllers();

app.Run();

