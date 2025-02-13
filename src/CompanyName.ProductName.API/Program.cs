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

// Create a new WebApplication builder instance
var builder = WebApplication.CreateBuilder(args);

// Configure Serilog as the logging provider
// This setup allows reading configuration from appsettings.json and registered services
builder.Host.UseSerilog((context, services, logger) =>
{
    logger.ReadFrom.Configuration(context.Configuration).ReadFrom.Services(services);
});

// Add essential services to the dependency injection container
builder.Services.AddControllers();  // Add MVC controllers
builder.Services.AddEndpointsApiExplorer();  // Add API explorer for endpoint documentation
builder.Services.AddSwaggerGen();  // Add Swagger/OpenAPI documentation generation

// Configure routing options
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;  // Force URLs to be lowercase
    options.LowercaseQueryStrings = true;  // Force query string parameters to be lowercase
});

// Register application services in the DI container
builder.Services.AddScoped<IAddressService, AddressService>();  // Address service with scoped lifetime

// Add memory cache service for caching capabilities
builder.Services.AddMemoryCache();

// Register HTTP message handlers
builder.Services.AddTransient<CachingIntegrationHandler>();  // Handler for caching HTTP responses
builder.Services.AddTransient<IntegrationHandler>();  // Handler for general HTTP integration concerns

// Configure Refit client for CEP (Brazilian postal code) integration
builder.Services.AddRefitClient<ICepIntegration>(options =>
    RefitSettingsFactory.CreateRefitSettings(
        options.GetRequiredService<ILoggerFactory>().CreateLogger("RefitClient"),
        nameof(ICepIntegration)
    ))
    .ConfigureHttpClient(c =>
    {
        // Set the base address from configuration
        c.BaseAddress = new Uri(builder.Configuration["CepIntegration:BaseAddress"]);
        c.Timeout = TimeSpan.FromSeconds(30); // Set timeout to 30 seconds
    })
    // Add retry policy for transient HTTP errors
    // Will retry 3 times with exponential backoff (2^attempt seconds)
    .AddPolicyHandler(HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
    // Add HTTP message handlers in the pipeline
    .AddHttpMessageHandler<IntegrationHandler>()
    .AddHttpMessageHandler<CachingIntegrationHandler>()
    // Set the handler lifetime to 5 minutes
    .SetHandlerLifetime(TimeSpan.FromMinutes(5));

// Build the application
var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();  // Enable Swagger endpoint
app.UseSwaggerUI();  // Enable Swagger UI

// Add global error handling middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

// Configure standard middleware components
app.UseHttpsRedirection();  // Redirect HTTP requests to HTTPS
app.UseAuthorization();  // Enable authorization
app.MapControllers();  // Map controller endpoints

// Start the application
await app.RunAsync();