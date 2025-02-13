using CompanyName.ProductName.Domain.Exceptions;
using CompanyName.ProductName.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CompanyName.ProductName.Tests.Infrastructure;

public class RefitSettingsFactoryTests
{
    private readonly Mock<ILogger> _loggerMock;

    public RefitSettingsFactoryTests()
    {
        _loggerMock = new Mock<ILogger>();
    }

    [Fact]
    public void CreateRefitSettings_ShouldReturnValidSettings()
    {
        // Act
        var settings = RefitSettingsFactory.CreateRefitSettings(_loggerMock.Object);

        // Assert
        Assert.NotNull(settings);
        Assert.NotNull(settings.ContentSerializer);
    }

    [Fact]
    public void CreateRefitSettings_WithNullLogger_ShouldThrowApiException()
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => RefitSettingsFactory.CreateRefitSettings(null));
    }
} 