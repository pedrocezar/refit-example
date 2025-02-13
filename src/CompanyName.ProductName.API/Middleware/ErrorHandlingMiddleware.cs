using System.Net;
using System.Text.Json;
using CompanyName.ProductName.Application.Contracts.Responses;
using CompanyName.ProductName.Domain.Exceptions;

namespace CompanyName.ProductName.API.Middleware;

public class ErrorHandlingMiddleware(RequestDelegate _next, ILogger<ErrorHandlingMiddleware> _logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new ErrorResponse
        {
            TraceId = context.TraceIdentifier
        };

        switch (exception)
        {
            case NotFoundException notFoundEx:
                response.StatusCode = 404;
                errorResponse.Message = notFoundEx.Message;
                break;

            case DomainException domainEx:
                response.StatusCode = 400;
                errorResponse.Message = domainEx.Message;
                break;

            case Refit.ApiException:
                response.StatusCode = 422;
                errorResponse.Message = "Unprocessable Entity";
                break;

            default:
                _logger.LogError(exception, "An unexpected error occurred");
                response.StatusCode = 500;
                errorResponse.Message = "An unexpected error occurred";
                break;
        }

        var result = JsonSerializer.Serialize(errorResponse, 
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await response.WriteAsync(result);
    }
}
