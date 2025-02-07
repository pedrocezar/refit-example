using CompanyName.ProductName.Application.Contracts.Responses;
using CompanyName.ProductName.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CompanyName.ProductName.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AddressController(IAddressService _adressService) : ControllerBase
{
    [HttpGet("cep/{cep}")]
    [ProducesResponseType(typeof(AddressResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AddressResponse>> GetByCep(string cep)
    {
        var address = await _adressService.GetAddressByCepAsync(cep);
        return Ok(address);
    }
}
