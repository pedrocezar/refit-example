using CompanyName.ProductName.Domain.Exceptions;
using CompanyName.ProductName.Infrastructure.Handlers;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CompanyName.ProductName.Tests.Infrastructure
{
    public class LoggingBehaviorTests
    {
        private readonly Mock<ILogger<LoggingBehavior<TestRequest, TestResponse>>> _loggerMock;
        private readonly LoggingBehavior<TestRequest, TestResponse> _loggingBehavior;

        public LoggingBehaviorTests()
        {
            _loggerMock = new Mock<ILogger<LoggingBehavior<TestRequest, TestResponse>>>();
            _loggingBehavior = new LoggingBehavior<TestRequest, TestResponse>(_loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldLogRequestAndResponse()
        {
            // Arrange
            var request = new TestRequest { Data = "Test" };
            var expectedResponse = new TestResponse { Result = "Success" };
            
            RequestHandlerDelegate<TestResponse> next = () => Task.FromResult(expectedResponse);

            // Act
            var response = await _loggingBehavior.Handle(request, next, CancellationToken.None);

            // Assert
            Assert.Equal(expectedResponse, response);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Handling")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Handled")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldLogException()
        {
            // Arrange
            var request = new TestRequest { Data = "Test" };
            var exception = new DomainException("Test exception");
            
            RequestHandlerDelegate<TestResponse> next = () => throw exception;

            // Act & Assert
            await Assert.ThrowsAsync<DomainException>(() => _loggingBehavior.Handle(request, next, CancellationToken.None));
        }

        public class TestRequest : IRequest<TestResponse>
        {
            public string Data { get; set; }
        }

        public class TestResponse
        {
            public string Result { get; set; }
        }
    }
}