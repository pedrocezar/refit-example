using CompanyName.ProductName.Application.Services;
using CompanyName.ProductName.Infrastructure.Handlers;
using CompanyName.ProductName.Infrastructure.Settings;
using CompanyName.ProductName.API.Middleware;
using CompanyName.ProductName.Application.Services.Interfaces;
using CompanyName.ProductName.Infrastructure.Integrations;
using Refit;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Logging - moved before builder.Build()
builder.Host.UseSerilog((context, services, logger) =>
{
    logger.ReadFrom.Configuration(context.Configuration).ReadFrom.Services(services);
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

// Configure Services
builder.Services.AddScoped<IAddressService, AddressService>();

// Configure Refit with logging and error handling
builder.Services.AddTransient<LoggingHandler>();

builder.Services.AddRefitClient<ICepIntegration>(options =>
    RefitSettingsFactory.CreateRefitSettings(
        options.GetRequiredService<ILoggerFactory>().CreateLogger("RefitClient")
    ))
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = new Uri(builder.Configuration["CepIntegration:BaseAddress"]);
        c.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddHttpMessageHandler<LoggingHandler>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(5));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

// Add error handling middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();