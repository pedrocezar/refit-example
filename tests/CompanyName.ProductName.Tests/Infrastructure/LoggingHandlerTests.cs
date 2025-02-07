using CompanyName.ProductName.Infrastructure.Handlers;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using Xunit;

namespace CompanyName.ProductName.Tests.Infrastructure;

public class LoggingHandlerTests
{
    private readonly Mock<ILogger<LoggingHandler>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _innerHandlerMock;
    private readonly HttpMessageHandler _handler;

    public LoggingHandlerTests()
    {
        _loggerMock = new Mock<ILogger<LoggingHandler>>();
        _innerHandlerMock = new Mock<HttpMessageHandler>();
        _handler = new LoggingHandler(_loggerMock.Object)
        {
            InnerHandler = _innerHandlerMock.Object
        };
    }

    [Fact]
    public async Task SendAsync_ShouldLogRequestAndResponse()
    {
        // Arrange
        var client = new HttpClient(_handler);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("test response")
        };

        _innerHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        await client.GetAsync("http://test.com");

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("HTTP Request")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("HTTP Response")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_WhenExceptionOccurs_ShouldLogError()
    {
        // Arrange
        var client = new HttpClient(_handler);
        var expectedException = new HttpRequestException("Test exception");

        _innerHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(expectedException);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await client.GetAsync("http://test.com"));

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("HTTP Request failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
} 