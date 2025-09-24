using System.Text.Json;
using System.Text.Json.Serialization;
using Fora.Application;
using Fora.Domain;
using Fora.Infrastructure;
using ForaApi.Infrastructure;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger/OpenAPI
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Fora API",
        Version = "v1",
        Description = "API for Fora application"
    });
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// Configuration
var edgarBaseUrl = builder.Configuration.GetValue<string>("Edgar:BaseUrl") ?? "https://data.sec.gov/api/xbrl/";

// Optional PathBase (for hosting under a sub-path like /tltest)
// Sources: appsettings PathBase or environment variable ASPNETCORE_PATHBASE
var configuredPathBase = builder.Configuration["PathBase"] ?? Environment.GetEnvironmentVariable("ASPNETCORE_PATHBASE");
if (!string.IsNullOrWhiteSpace(configuredPathBase) && !configuredPathBase!.StartsWith('/'))
{
    configuredPathBase = "/" + configuredPathBase;
}

// EF Core SQLite
builder.Services.AddDbContext<AppDbContext>(opts =>
{
    opts.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=app.db");
});

builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddSingleton<IFundingCalculator, FundingCalculator>();

// JSON options singleton to share with EdgarClient
builder.Services.AddSingleton(new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
});

// Polly retry + 429/backoff handling with jitter
var delay = Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), retryCount: 3);
var retryPolicy = Policy.HandleResult<HttpResponseMessage>(r =>
        (int)r.StatusCode == 429 || (int)r.StatusCode >= 500)
    .WaitAndRetryAsync(delay);

builder.Services.AddHttpClient<IEdgarClient, EdgarClient>(client =>
{
    client.BaseAddress = new Uri(edgarBaseUrl);
    client.DefaultRequestHeaders.Add("User-Agent", "PostmanRuntime/7.34.0");
    client.DefaultRequestHeaders.Add("Accept", "*/*");
}).AddPolicyHandler(retryPolicy);

builder.Services.AddScoped<IImporter, Importer>();

// Apply migrations and optional import via hosted service
builder.Services.AddHostedService<DatabaseMigrationHostedService>();

var app = builder.Build();

// Forwarded headers (for reverse proxy / TLS termination)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
});

// Apply PathBase when configured (e.g., "/tltest")
if (!string.IsNullOrWhiteSpace(configuredPathBase))
{
    app.UsePathBase(configuredPathBase);
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var problem = Results.Problem(detail: exceptionHandlerPathFeature?.Error.Message, statusCode: 500);
        await problem.ExecuteAsync(context);
    });
});

// Enable Swagger UI (serve at root / and /index.html under the PathBase if any)
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    // Use a relative endpoint so it works under a PathBase (e.g., /tltest)
    options.SwaggerEndpoint("swagger/v1/swagger.json", "Fora API v1");
    options.RoutePrefix = string.Empty; // UI at / (within PathBase)
});

app.UseHttpsRedirection();

// Map attribute-routed controllers (e.g., WeatherForecastController, CompaniesController, AdminController)
app.MapControllers();

app.Run();
