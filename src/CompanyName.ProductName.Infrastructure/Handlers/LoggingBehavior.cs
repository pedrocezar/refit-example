using CompanyName.ProductName.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CompanyName.ProductName.Infrastructure.Handlers;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> _logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString();

        try
        {
            _logger.LogInformation(
                "Handling {RequestName} - CorrelationId: {CorrelationId} - Request: {@Request}",
                requestName,
                correlationId,
                request);

            var stopwatch = Stopwatch.StartNew();
            var response = await next();
            stopwatch.Stop();

            _logger.LogInformation(
                "Handled {RequestName} in {ElapsedMilliseconds}ms - CorrelationId: {CorrelationId} - Response: {@Response}",
                requestName,
                stopwatch.ElapsedMilliseconds,
                correlationId,
                response);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error handling {RequestName} - CorrelationId: {CorrelationId} - Error: {Error}",
                requestName,
                correlationId,
                ex.Message);
            throw new DomainException($"Error handling {requestName} - CorrelationId: {correlationId}");
        }
    }
}
