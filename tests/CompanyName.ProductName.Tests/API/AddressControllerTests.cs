using CompanyName.ProductName.API.Controllers;
using CompanyName.ProductName.Application.Contracts.Responses;
using CompanyName.ProductName.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CompanyName.ProductName.Tests.API;

public class AddressControllerTests
{
    private readonly Mock<IAddressService> _addressServiceMock;
    private readonly AddressController _controller;

    public AddressControllerTests()
    {
        _addressServiceMock = new Mock<IAddressService>();
        _controller = new AddressController(_addressServiceMock.Object);
    }

    [Fact]
    public async Task GetByCep_WithValidCep_ShouldReturnOk()
    {
        // Arrange
        var cep = "01001000";
        var expectedResponse = new AddressResponse
        {
            ZipCode = cep,
            City = "São Paulo",
            State = "SP",
            Street = "Praça da Sé",
            Neighborhood = "Sé"
        };

        _addressServiceMock.Setup(x => x.GetAddressByCepAsync(cep))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetByCep(cep);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<AddressResponse>(okResult.Value);
        Assert.Equal(expectedResponse.ZipCode, returnValue.ZipCode);
        Assert.Equal(expectedResponse.City, returnValue.City);
        Assert.Equal(expectedResponse.State, returnValue.State);
    }
} 