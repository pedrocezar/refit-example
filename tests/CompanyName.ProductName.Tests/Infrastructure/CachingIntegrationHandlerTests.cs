using CompanyName.ProductName.Infrastructure.Handlers;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.Protected;
using System.Net;
using Xunit;

namespace CompanyName.ProductName.API.Tests.Handlers
{
    public class CachingIntegrationHandlerTests
    {
        private readonly IMemoryCache _cache;
        private readonly Mock<HttpMessageHandler> _innerHandlerMock;
        private readonly HttpMessageInvoker _invoker;

        public CachingIntegrationHandlerTests()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
            _innerHandlerMock = new Mock<HttpMessageHandler>();
            var handler = new CachingIntegrationHandler(_cache)
            {
                InnerHandler = _innerHandlerMock.Object
            };
            _invoker = new HttpMessageInvoker(handler);
        }

        [Fact]
        public async Task SendAsync_WhenGetRequest_ShouldCacheResponse()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Test response")
            };

            _innerHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(expectedResponse);

            // Act
            var firstResponse = await _invoker.SendAsync(request, CancellationToken.None);
            var secondResponse = await _invoker.SendAsync(request, CancellationToken.None);

            // Assert
            Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
            Assert.Equal(
                await firstResponse.Content.ReadAsStringAsync(),
                await secondResponse.Content.ReadAsStringAsync()
            );

            _innerHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task SendAsync_WhenPostRequest_ShouldNotCacheResponse()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.example.com/test");
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Test response")
            };

            _innerHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(expectedResponse);

            // Act
            var firstResponse = await _invoker.SendAsync(request, CancellationToken.None);
            var secondResponse = await _invoker.SendAsync(request, CancellationToken.None);

            // Assert
            Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
            Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
            
            // Verify inner handler was called twice since POST requests aren't cached
            _innerHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(2),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}