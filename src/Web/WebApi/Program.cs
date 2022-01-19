using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Console;
using Serilog;
using Xunkong.Core.XunkongApi;
using Xunkong.Web.Api.Controllers;
using Xunkong.Web.Api.Filters;
using Xunkong.Web.Api.Services;

// net6限定：避免因为启用剪裁，在efcore的使用中出现「找不到System.DateOnly相关方法」的异常
DateOnly.FromDayNumber(1).AddYears(1).AddMonths(1).AddDays(1);
StateController.StartTime = DateTime.Now;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ControllerExceptionFilter>();
});
//builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v0.1", new()
    {
        Title = "Xunkong Web Api",
        Version = "v0.1",
        Description = "刻记牛杂店的公开Api（v0为开发和测试版本）",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Github",
            Url = new Uri("https://github.com/Scighost/Xunkong")
        },
    });
    c.SwaggerDoc("v1", new()
    {
        Title = "Xunkong Web Api",
        Version = "v1",
        Description = "刻记牛杂店的公开Api（v1正式版）\n\n因个人能力有限（指边学边做），不能保证服务的安全和稳定（云服务商线路挂了别怪我）",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Github",
            Url = new Uri("https://github.com/Scighost/Xunkong")
        },
    });
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Xunkong.Web.Api.xml"), true);
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
builder.Services.AddScoped<WishlogResultFilter>();
builder.Services.AddDbContextPool<XunkongDbContext>(options =>
{
#if DEBUG
    options.UseMySql(builder.Configuration.GetConnectionString("constr_xunkong"), new MySqlServerVersion("8.0.27"));
#else
    options.UseMySql(Environment.GetEnvironmentVariable("CONSTR"), new MySqlServerVersion("8.0.25"));
#endif
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.AllowTrailingCommas = true;
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip;
});

builder.Services.AddLogging(builder => builder.AddSimpleConsole(c => { c.ColorBehavior = LoggerColorBehavior.Enabled; c.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff zzz\n"; }));

builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

builder.Services.Configure<ApiBehaviorOptions>(options => options.InvalidModelStateResponseFactory = (context) => throw new XunkongApiException(ReturnCode.InvalidModelException));

builder.Host.UseSerilog((ctx, config) => config.ReadFrom.Configuration(ctx.Configuration));




var app = builder.Build();

// 异常捕获
app.UseExceptionHandler(c => c.Run(async context =>
{
    var feature = context.Features.Get<IExceptionHandlerPathFeature>();
    var ex = feature?.Error;
    app.Services.GetService<Microsoft.Extensions.Logging.ILogger>()?.LogError(ex, "Exception middleware");
    var result = new ResponseData(ReturnCode.InternalException, ex?.Message, new ExceptionResult(ex?.GetType()?.ToString(), ex?.Message, ex?.StackTrace));
    context.Response.StatusCode = 500;
    await context.Response.WriteAsJsonAsync(result);
}));

// 阻止http请求
app.Use(async (context, next) =>
{
    if (context.Request.Headers["X-Forwarded-Proto"] == "http")
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return;
    }
    await next.Invoke();
});

app.UseResponseCompression();

//app.UseHttpLogging();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Xunkong Web Api v1");
        c.SwaggerEndpoint("/swagger/v0.1/swagger.json", "Xunkong Web Api v0.1");
    });
}

app.UseAuthorization();

app.MapControllers();

app.Run();

