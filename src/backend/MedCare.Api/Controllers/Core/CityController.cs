using MedCare.Application.Abstractions.Services.Core;
using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace MedCare.Api.Controllers.Core;
[Route("api/[controller]")]
[ApiController]
public class CityController(ICityService cityService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CityReqDTO city)
    {
        var result = await cityService.CreateCityAsync(city);
        return CreatedAtAction(nameof(GetCityById), new { cityId = result },
            ApiResponse<int>.Created(result, "City registered successfully."));
    }

    [HttpPut("{cityId:int}")]
    [ProducesResponseType(typeof(ApiResponse<CityReqDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int cityId, [FromBody] CityReqDTO city)
    {
        await cityService.UpdateCityAsync(city, cityId);
        var updatedCity = await cityService.GetCityByIdAsync(cityId);
        return Ok(ApiResponse<CityReqDTO>.Success(updatedCity, "City updated successfully."));
    }

    [HttpGet("{cityId:int}")]
    [ProducesResponseType(typeof(ApiResponse<CityReqDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCityById(int cityId)
    {
        var result = await cityService.GetCityByIdAsync(cityId);
        return Ok(ApiResponse<CityReqDTO>.Success(result, "City fetched successfully."));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CityReqDTO>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PagedResult<CityRespDTO>>>> GetCityByPage([FromQuery, StringLength(100)] string? search, [FromQuery, StringLength(20)] string? sortBy, [FromQuery, RegularExpression("(?i)^(asc|desc)$")] string? sortDirection, [FromQuery, Range(1, int.MaxValue)] int page = 1, [FromQuery, Range(1, 100)] int pageSize = 20, CancellationToken ct = default)
    {
        var query = new PageQuery(search, sortBy, sortDirection, page, pageSize);
        var result = await cityService.GetCityByPageAsync(query, ct);

        return Ok(ApiResponse<PagedResult<CityRespDTO>>.Success(result, "City list fetched successfully."));
    }

    [HttpDelete("{cityId:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int cityId)
    {
        await cityService.DeleteCityAsync(cityId);
        return Ok(ApiResponse.Success("City deleted successfully."));
    }
}
