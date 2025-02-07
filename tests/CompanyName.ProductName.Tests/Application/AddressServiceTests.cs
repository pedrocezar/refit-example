using CompanyName.ProductName.Application.Contracts.Responses;
using CompanyName.ProductName.Application.Services;
using CompanyName.ProductName.Domain.Models;
using CompanyName.ProductName.Infrastructure.Integrations;
using Moq;
using Xunit;

namespace CompanyName.ProductName.Tests.Application;

public class AddressServiceTests
{
    private readonly Mock<ICepIntegration> _cepIntegrationMock;
    private readonly AddressService _service;

    public AddressServiceTests()
    {
        _cepIntegrationMock = new Mock<ICepIntegration>();
        _service = new AddressService(_cepIntegrationMock.Object);
    }

    [Fact]
    public async Task GetAddressByCepAsync_WithValidCep_ShouldReturnAddress()
    {
        // Arrange
        var cep = "01001000";
        var cepModel = new CepModel
        {
            Cep = cep,
            Logradouro = "Praça da Sé",
            Bairro = "Sé",
            Localidade = "São Paulo",
            Uf = "SP"
        };

        _cepIntegrationMock.Setup(x => x.GetAddressByCepAsync(cep))
            .ReturnsAsync(cepModel);

        // Act
        var result = await _service.GetAddressByCepAsync(cep);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<AddressResponse>(result);
        Assert.Equal(cepModel.Cep, result.ZipCode);
        Assert.Equal(cepModel.Logradouro, result.Street);
        Assert.Equal(cepModel.Bairro, result.Neighborhood);
        Assert.Equal(cepModel.Localidade, result.City);
        Assert.Equal(cepModel.Uf, result.State);
    }
} 