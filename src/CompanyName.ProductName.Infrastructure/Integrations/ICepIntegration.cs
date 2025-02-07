using CompanyName.ProductName.Domain.Models;
using Refit;

namespace CompanyName.ProductName.Infrastructure.Integrations;

public interface ICepIntegration
{
    [Get("/ws/{cep}/json")]
    Task<CepModel> GetAddressByCepAsync(string cep);
}
