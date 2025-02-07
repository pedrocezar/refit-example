using CompanyName.ProductName.Domain.Models;

namespace CompanyName.ProductName.Application.Contracts.Responses;

public class AddressResponse
{
    public AddressResponse() { }

    public AddressResponse(CepModel model)
    {
        City = model.Localidade;
        Neighborhood = model.Bairro;
        State = model.Uf;
        Street = model.Logradouro;
        ZipCode = model.Cep;
    }

    public string ZipCode { get; set; }
    public string Street { get; set; }
    public string Neighborhood { get; set; }
    public string City { get; set; }
    public string State { get; set; }
}