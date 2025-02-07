using CompanyName.ProductName.Application.Contracts.Responses;
using CompanyName.ProductName.Application.Services.Interfaces;
using CompanyName.ProductName.Domain.Exceptions;
using CompanyName.ProductName.Infrastructure.Integrations;

namespace CompanyName.ProductName.Application.Services;

public class AddressService(ICepIntegration _cepIntegration) : IAddressService
{
    public async Task<AddressResponse> GetAddressByCepAsync(string cep)
    {
        var result = await _cepIntegration.GetAddressByCepAsync(cep);

        if (result.Erro.GetValueOrDefault())
            throw new IntegrationException(nameof(ICepIntegration), "Unexpected error");

        return new AddressResponse(result);
    }
}