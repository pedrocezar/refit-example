using CompanyName.ProductName.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CompanyName.ProductName.Infrastructure.Handlers;

public class IntegrationHandler(ILogger<IntegrationHandler> _logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString();

        try
        {
            _logger.LogInformation(
                "HTTP Request {Method} {Uri} started. CorrelationId: {CorrelationId}",
                request.Method,
                request.RequestUri,
                correlationId);


            var stopwatch = Stopwatch.StartNew();

            request.Headers.Add("Authorization", "***WRITE YOUR API READ ACCESS TOKEN HERE***");
            request.Headers.Add("CorrelationId", correlationId);

            var response = await base.SendAsync(request, cancellationToken);

            stopwatch.Stop();

            _logger.LogInformation(
                "HTTP Response {Method} {Uri} finished in {ElapsedMilliseconds}ms with status code {StatusCode}. CorrelationId: {CorrelationId}",
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
                "HTTP Request failed. Method: {Method}, Uri: {Uri}, CorrelationId: {CorrelationId}",
                request.Method,
                request.RequestUri,
                correlationId);
            throw new DomainException($"Error handling {request.Method} - {request.RequestUri} - CorrelationId: {correlationId}");
        }
    }
}