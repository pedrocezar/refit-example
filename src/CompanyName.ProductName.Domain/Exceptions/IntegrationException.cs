namespace CompanyName.ProductName.Domain.Exceptions;

public class IntegrationException : DomainException
{
    public string ServiceName { get; }
    public int? StatusCode { get; }

    public IntegrationException(string serviceName, string message, int? statusCode = null)
        : base(message)
    {
        ServiceName = serviceName;
        StatusCode = statusCode;
    }
}
