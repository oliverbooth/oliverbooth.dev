using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using OliverBooth.Api;
using OliverBooth.Common.Extensions;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("localhost", policy => policy.WithOrigins("https://localhost:2845"));
    options.AddPolicy("site", policy => policy.WithOrigins("https://oliverbooth.dev"));
});

builder.Services.AddCommonServices();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.ResolveConflictingActions(resolver =>
{
    foreach (ApiDescription description in resolver)
    {
        if (description.GetApiVersion()?.MajorVersion == 2)
        {
            return description;
        }
    }

    return null;
}));
builder.Services.AddControllers();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(2);
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

if (builder.Environment.IsProduction())
{
    builder.WebHost.AddCertificateFromEnvironment(2844, 5048);
}

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
        {
            var url = $"/swagger/{description.GroupName}/swagger.json";
            options.SwaggerEndpoint(url, description.GroupName.ToUpperInvariant());
        }
    });
}

app.UseHttpsRedirection();
app.MapControllers();
app.UseCors(app.Environment.IsDevelopment() ? "localhost" : "site");

app.Run();
