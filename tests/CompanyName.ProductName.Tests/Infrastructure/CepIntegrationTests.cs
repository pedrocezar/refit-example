using CompanyName.ProductName.Infrastructure.Handlers;
using CompanyName.ProductName.Infrastructure.Integrations;
using CompanyName.ProductName.Infrastructure.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Refit;
using Xunit;

namespace CompanyName.ProductName.Tests.Infrastructure
{
    public class CepServiceTests
    {
        private readonly ICepIntegration _cepService;
        private readonly Mock<ILogger<IntegrationHandler>> _loggerMock;

        public CepServiceTests()
        {
            _loggerMock = new Mock<ILogger<IntegrationHandler>>();
            
            var services = new ServiceCollection();
            services.AddSingleton(_loggerMock.Object);
            services.AddTransient<IntegrationHandler>();
            
            services.AddRefitClient<ICepIntegration>(RefitSettingsFactory.CreateRefitSettings(_loggerMock.Object, nameof(ICepIntegration)))
                .ConfigureHttpClient(c => 
                {
                    c.BaseAddress = new Uri("https://viacep.com.br");
                    c.Timeout = TimeSpan.FromSeconds(30);
                })
                .AddHttpMessageHandler<IntegrationHandler>();

            var serviceProvider = services.BuildServiceProvider();
            _cepService = serviceProvider.GetRequiredService<ICepIntegration>();
        }

        [Fact]
        public async Task GetAddressByCep_WithValidCep_ShouldReturnAddress()
        {
            // Arrange
            var cep = "01001000";

            // Act
            var result = await _cepService.GetAddressByCepAsync(cep);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("SP", result.Uf);
            Assert.Equal("São Paulo", result.Localidade);

            VerifyLogging();
        }

        [Fact]
        public async Task GetAddressByCep_WithInvalidCep_ShouldThrowApiException()
        {
            // Arrange
            var invalidCep = "00000000";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApiException>(
                () => _cepService.GetAddressByCepAsync(invalidCep));
            
            Assert.Contains("an error occured deserializing the response", exception.Message.ToLower());
            VerifyLogging();
        }

        private void VerifyLogging()
        {
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("started")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.Once);
        }
    }
} 