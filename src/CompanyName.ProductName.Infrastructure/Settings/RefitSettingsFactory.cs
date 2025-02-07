using System.Net;
using CompanyName.ProductName.Domain.Exceptions;
using CompanyName.ProductName.Domain.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Refit;

namespace CompanyName.ProductName.Infrastructure.Settings;

public static class RefitSettingsFactory
{
    public static RefitSettings CreateRefitSettings(ILogger logger)
    {
        return new RefitSettings
        {
            ExceptionFactory = async message =>
            {
                if (message.IsSuccessStatusCode)
                    return null;

                var statusCode = message.StatusCode;
                var content = await message.Content.ReadAsStringAsync();

                logger.LogError("External API error: Status Code: {StatusCode}, Content: {Content}",
                    statusCode, content);

                var errorMessage = GetErrorMessage(content, statusCode);

                return statusCode switch
                {
                    HttpStatusCode.NotFound => new NotFoundException(errorMessage),
                    HttpStatusCode.BadRequest => new DomainException(errorMessage),
                    _ => new IntegrationException(
                        "ViaCEP",
                        errorMessage,
                        (int)statusCode)
                };
            }
        };
    }

    private static string GetErrorMessage(string content, HttpStatusCode statusCode)
    {
        if (string.IsNullOrWhiteSpace(content))
            return $"External service error with status code {statusCode}";

        try
        {
            var errorResponse = JsonConvert.DeserializeObject<ErrorModel>(content);
            return errorResponse?.Error ?? content;
        }
        catch
        {
            return content;
        }
    }
}