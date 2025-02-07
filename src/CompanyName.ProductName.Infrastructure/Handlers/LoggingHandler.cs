using CompanyName.ProductName.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CompanyName.ProductName.Infrastructure.Handlers;

public class LoggingHandler(ILogger<LoggingHandler> _logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString();

        try
        {
            _logger.LogInformation(
                "HTTP {Method} {Uri} started. CorrelationId: {CorrelationId}",
                request.Method,
                request.RequestUri,
                correlationId);

            var stopwatch = Stopwatch.StartNew();
            var response = await base.SendAsync(request, cancellationToken);
            stopwatch.Stop();

            _logger.LogInformation(
                "HTTP {Method} {Uri} finished in {ElapsedMilliseconds}ms with status code {StatusCode}. CorrelationId: {CorrelationId}",
                request.Method,
                request.RequestUri,
                stopwatch.ElapsedMilliseconds,
                response.StatusCode,
                correlationId);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "HTTP {Method} {Uri} failed. CorrelationId: {CorrelationId}",
                request.Method,
                request.RequestUri,
                correlationId);
            throw new DomainException($"Error handling {request.Method} - {request.RequestUri} - CorrelationId: {correlationId}");
        }
    }
}