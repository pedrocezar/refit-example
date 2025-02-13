using CompanyName.ProductName.Application.Services;
using CompanyName.ProductName.Infrastructure.Handlers;
using CompanyName.ProductName.Infrastructure.Settings;
using CompanyName.ProductName.API.Middleware;
using CompanyName.ProductName.Application.Services.Interfaces;
using CompanyName.ProductName.Infrastructure.Integrations;
using Refit;
using Serilog;
using Polly.Extensions.Http;
using Polly;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, logger) =>
{
    logger.ReadFrom.Configuration(context.Configuration).ReadFrom.Services(services);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

builder.Services.AddScoped<IAddressService, AddressService>();

builder.Services.AddMemoryCache();

builder.Services.AddTransient<CachingIntegrationHandler>();
builder.Services.AddTransient<IntegrationHandler>();

builder.Services.AddRefitClient<ICepIntegration>(options =>
    RefitSettingsFactory.CreateRefitSettings(
        options.GetRequiredService<ILoggerFactory>().CreateLogger("RefitClient"),
        nameof(ICepIntegration)
    ))
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = new Uri(builder.Configuration["CepIntegration:BaseAddress"]);
        c.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddPolicyHandler(HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
    .AddHttpMessageHandler<IntegrationHandler>()
    .AddHttpMessageHandler<CachingIntegrationHandler>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(5));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();