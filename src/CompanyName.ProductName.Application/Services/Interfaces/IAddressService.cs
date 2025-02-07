using CompanyName.ProductName.Application.Contracts.Responses;

namespace CompanyName.ProductName.Application.Services.Interfaces;

public interface IAddressService
{
    Task<AddressResponse> GetAddressByCepAsync(string cep);
}