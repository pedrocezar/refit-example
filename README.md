# CEP Integration Service

A .NET 8 Web API service that integrates with the ViaCEP API to provide Brazilian postal code (CEP) lookup functionality. Built using Clean Architecture principles and modern .NET practices.

## 🚀 Features

- CEP (Brazilian postal code) lookup and validation
- Seamless integration with ViaCEP API using Refit
- Comprehensive error handling with middleware
- Structured logging with Serilog (console, file)
- Swagger/OpenAPI documentation
- Clean Architecture implementation
- Unit and integration tests
- Dependency injection
- CQRS pattern with MediatR

## 🏗️ Architecture

The project follows Clean Architecture principles with the following structure:

```

src/
├── CompanyName.ProductName.API/           # API Layer
│   ├── Controllers/                       # API Controllers
│   ├── Middleware/                        # Custom Middleware
│   └── Configuration/                     # App Configuration
│
├── CompanyName.ProductName.Application/   # Application Layer
│   ├── Services/                          # Application Services
│   ├── Contracts/                         # DTOs, Interfaces
│   └── Validators/                        # Input Validation
│
├── CompanyName.ProductName.Domain/        # Domain Layer
│   ├── Models/                            # Domain Models
│   ├── Exceptions/                        # Custom Exceptions
│   └── Interfaces/                        # Core Interfaces
│
└── CompanyName.ProductName.Infrastructure/# Infrastructure Layer
    ├── Integrations/                      # External Services
    ├── Handlers/                          # Request Handlers
    └── Settings/                          # Infrastructure Config
```

## 🛠️ Technical Stack

- **Framework**: .NET 8
- **API Integration**: Refit 8.0
- **Logging**: Serilog 4.2.0
  - Console sink
  - File sink with daily rolling
  - Environment enrichers
- **Documentation**: Swagger/OpenAPI
- **Testing**:
  - xUnit
  - Moq
  - FluentAssertions
- **Error Handling**: Global exception middleware
- **Patterns**: CQRS, Repository, DI

## 📋 Prerequisites

- .NET 8 SDK
- An IDE (Visual Studio 2022, VS Code, or JetBrains Rider)
- Git

## ⚙️ Configuration

### Application Settings (appsettings.json)

```json
{
  "CepIntegration": {
    "BaseAddress": "https://viacep.com.br/ws/"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/log-.txt",
          "rollingInterval": "Day"
        }
      }
    ]
  }
}
```

## 🔧 Implementation Details

### Refit Integration

```csharp
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
```

## 🧪 Detailed Test Structure

The project includes comprehensive test coverage across all layers:

### Infrastructure Tests

#### LoggingHandlerTests
Tests the custom HTTP message handler that provides logging for HTTP requests:
- Logs HTTP request details
- Logs HTTP response details
- Handles different response scenarios

#### RefitSettingsFactoryTests
Validates the Refit client configuration:
- Creates correct serialization settings
- Configures proper content handlers
- Validates error handling settings

### API Tests

#### AddressControllerTests
Tests the Address API endpoints:
- Successful CEP lookup
- Invalid CEP format handling
- Not found CEP scenarios
- Error response format validation

#### ErrorHandlingMiddlewareTests
Validates the global error handling:
- Catches and handles NotFoundException
- Processes DomainException correctly
- Handles unexpected errors
- Validates error response format

### Application Tests

#### AddressServiceTests
Tests the core address service functionality:
- Successful address retrieval
- Input validation
- Error handling
- Response mapping

### Integration Tests

#### CepIntegrationTests
End-to-end tests for the ViaCEP integration:
- Valid CEP requests
- Invalid CEP handling
- Timeout scenarios
- Network error handling

### Test Project Structure
```

tests/
└── CompanyName.ProductName.Tests/
    ├── API/
    │   ├── AddressControllerTests.cs
    │   └── ErrorHandlingMiddlewareTests.cs
    ├── Application/
    │   └── AddressServiceTests.cs
    └── Infrastructure/
        ├── CepIntegrationTests.cs
        ├── LoggingHandlerTests.cs
        └── RefitSettingsFactoryTests.cs
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run only API tests
dotnet test --filter "Category=API"

# Run only Integration tests
dotnet test --filter "Category=Integration"

# Run specific test class
dotnet test --filter "FullyQualifiedName~AddressControllerTests"
```

## 🚦 API Endpoints

### Get Address by CEP

```http
GET /api/address/cep/{cep}
```

#### Parameters
- `cep` (string, required): Brazilian postal code in format: 00000000 or 00000-000

#### Responses

##### 200 OK
```json
{
  "zipCode": "01001000",
  "street": "Praça da Sé",
  "neighborhood": "Sé",
  "city": "São Paulo",
  "state": "SP"
}
```

##### 404 Not Found
```json
{
  "message": "CEP not found",
  "traceId": "00-1234...5678"
}
```

## 📝 Logging

Logging is implemented using Serilog with structured logging format:

```json
{
  "Timestamp": "2024-01-20T10:00:00.000Z",
  "Level": "Information",
  "MessageTemplate": "Request completed in {Elapsed} ms",
  "Properties": {
    "Elapsed": 125,
    "StatusCode": 200,
    "Method": "GET",
    "Path": "/api/address/cep/01001000"
  }
}
```

## 🔒 Security Considerations

- HTTPS enforcement
- Input validation
- Error message sanitization
- Logging best practices
- Rate limiting capabilities

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request