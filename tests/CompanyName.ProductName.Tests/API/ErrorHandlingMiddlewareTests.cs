using CompanyName.ProductName.API.Middleware;
using CompanyName.ProductName.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

namespace CompanyName.ProductName.Tests.API;

public class ErrorHandlingMiddlewareTests
{
    private readonly Mock<ILogger<ErrorHandlingMiddleware>> _loggerMock;
    private readonly ErrorHandlingMiddleware _middleware;
    private readonly HttpContext _httpContext;
    private readonly Mock<RequestDelegate> _nextMock;

    public ErrorHandlingMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<ErrorHandlingMiddleware>>();
        _nextMock = new Mock<RequestDelegate>();
        _middleware = new ErrorHandlingMiddleware(_nextMock.Object, _loggerMock.Object);
        
        _httpContext = new DefaultHttpContext();
        _httpContext.Response.Body = new MemoryStream();
    }

    [Fact]
    public async Task InvokeAsync_WithNotFoundException_ShouldReturn404()
    {
        // Arrange
        var errorMessage = "Address not found";
        _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
            .ThrowsAsync(new NotFoundException(errorMessage));

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, _httpContext.Response.StatusCode);
        
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var error = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody);
        
        Assert.Equal(errorMessage, error["message"]);
        Assert.NotNull(error["traceId"]);
    }

    [Fact]
    public async Task InvokeAsync_WithUnexpectedException_ShouldReturn500()
    {
        // Arrange
        _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, _httpContext.Response.StatusCode);
        
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var error = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody);
        
        Assert.Equal("An unexpected error occurred", error["message"]);
        Assert.NotNull(error["traceId"]);
        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
} 