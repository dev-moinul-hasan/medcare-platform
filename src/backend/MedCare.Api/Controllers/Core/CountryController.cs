using MedCare.Application.Abstractions.Services.Core;
using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace MedCare.Api.Controllers.Core;
[Route("api/[controller]")]
[ApiController]
public class CountryController(ICountryService countryService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CountryReqDTO country)
    {
        var result = await countryService.CreateCountryAsync(country);
        return CreatedAtAction(nameof(GetCountryById), new { countryId = result },
            ApiResponse<int>.Created(result, "Country registered successfully."));
    }

    [HttpPut("{countryId:int}")]
    [ProducesResponseType(typeof(ApiResponse<CountryReqDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int countryId, [FromBody] CountryReqDTO country)
    {
        await countryService.UpdateCountryAsync(country, countryId);
        var updatedCountry = await countryService.GetCountryByIdAsync(countryId);
        return Ok(ApiResponse<CountryReqDTO>.Success(updatedCountry, "Country updated successfully."));
    }

    [HttpGet("{countryId:int}")]
    [ProducesResponseType(typeof(ApiResponse<CountryReqDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCountryById(int countryId)
    {
        var result = await countryService.GetCountryByIdAsync(countryId);
        return Ok(ApiResponse<CountryReqDTO>.Success(result, "Country fetched successfully."));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CountryReqDTO>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PagedResult<CountryRespDTO>>>> GetCountryByPage([FromQuery, StringLength(100)] string? search, [FromQuery, StringLength(20)] string? sortBy, [FromQuery, RegularExpression("(?i)^(asc|desc)$")] string? sortDirection, [FromQuery, Range(1, int.MaxValue)] int page = 1, [FromQuery, Range(1, 100)] int pageSize = 20, CancellationToken ct = default)
    {
        var query = new PageQuery(search, sortBy, sortDirection, page, pageSize);
        var result = await countryService.GetCountryByPageAsync(query, ct);

        return Ok(ApiResponse<PagedResult<CountryRespDTO>>.Success(result, "Country list fetched successfully."));
    }

    [HttpDelete("{countryId:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int countryId)
    {
        await countryService.DeleteCountryAsync(countryId);
        return Ok(ApiResponse.Success("Country deleted successfully."));
    }
}
