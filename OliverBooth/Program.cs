using System.Security.Authentication;
using Asp.Versioning;
using AspNetCore.ReCaptcha;
using FluentFTP;
using FluentFTP.Logging;
using Markdig;
using OliverBooth.Common.Extensions;
using OliverBooth.Common.Markdown.Template;
using OliverBooth.Common.Services;
using OliverBooth.Markdown.Timestamp;
using OliverBooth.Services;
using Serilog;
using Serilog.Extensions.Logging;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/latest.log", rollingInterval: RollingInterval.Day)
#if DEBUG
    .MinimumLevel.Debug()
#endif
    .CreateLogger();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddTomlFile("data/config.toml", true, true);
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

builder.Services.AddSingleton(provider => new MarkdownPipelineBuilder()
    .Use<TimestampExtension>()
    .Use(new TemplateExtension(provider.GetRequiredService<ITemplateService>()))
    .UseAdvancedExtensions()
    .UseBootstrap()
    .UseEmojiAndSmiley()
    .UseSmartyPants()
    .Build());

builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1);
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

builder.Services.AddCommonServices();
builder.Services.AddHttpClient();
builder.Services.AddTransient<IAsyncFtpClient, AsyncFtpClient>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    string? host = configuration["Cdn:Ftp:Host"];
    string? username = configuration["Cdn:Ftp:Username"];
    string? password = configuration["Cdn:Ftp:Password"];

    if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
    {
        throw new AuthenticationException("Configuration value missing for CDN FTP.");
    }

    var client = new AsyncFtpClient(host, username, password);
    var loggerFactory = new SerilogLoggerFactory(Log.Logger);
    client.Logger = new FtpLogAdapter(loggerFactory.CreateLogger("FTP"));
    return client;
});

builder.Services.AddSingleton<ICdnService, CdnService>();
builder.Services.AddSingleton<IMastodonService, MastodonService>();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddServerSideBlazor().AddInteractiveServerComponents();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddReCaptcha(builder.Configuration.GetSection("ReCaptcha"));

if (builder.Environment.IsProduction())
{
    builder.WebHost.AddCertificateFromEnvironment(2845, 5049);
}

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStatusCodePagesWithRedirects("/error/{0}");
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();
app.MapBlazorHub();

app.Run();
