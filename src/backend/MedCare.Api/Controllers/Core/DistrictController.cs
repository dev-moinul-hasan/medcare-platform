using MedCare.Application.Abstractions.Services.Core;
using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace MedCare.Api.Controllers.Core;
[Route("api/[controller]")]
[ApiController]
public class DistrictController(IDistrictService districtService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] DistrictReqDTO district)
    {
        var result = await districtService.CreateDistrictAsync(district);
        return CreatedAtAction(nameof(GetDistrictById), new { districtId = result },
            ApiResponse<int>.Created(result, "District registered successfully."));
    }

    [HttpPut("{districtId:int}")]
    [ProducesResponseType(typeof(ApiResponse<DistrictReqDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int districtId, [FromBody] DistrictReqDTO district)
    {
        await districtService.UpdateDistrictAsync(district, districtId);
        var updatedDistrict = await districtService.GetDistrictByIdAsync(districtId);
        return Ok(ApiResponse<DistrictReqDTO>.Success(updatedDistrict, "District updated successfully."));
    }

    [HttpGet("{districtId:int}")]
    [ProducesResponseType(typeof(ApiResponse<DistrictReqDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDistrictById(int districtId)
    {
        var result = await districtService.GetDistrictByIdAsync(districtId);
        return Ok(ApiResponse<DistrictReqDTO>.Success(result, "District fetched successfully."));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DistrictReqDTO>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PagedResult<DistrictRespDTO>>>> GetDistrictByPage([FromQuery, StringLength(100)] string? search, [FromQuery, StringLength(20)] string? sortBy, [FromQuery, RegularExpression("(?i)^(asc|desc)$")] string? sortDirection, [FromQuery, Range(1, int.MaxValue)] int page = 1, [FromQuery, Range(1, 100)] int pageSize = 20, CancellationToken ct = default)
    {
        var query = new PageQuery(search, sortBy, sortDirection, page, pageSize);
        var result = await districtService.GetDistrictByPageAsync(query, ct);

        return Ok(ApiResponse<PagedResult<DistrictRespDTO>>.Success(result, "District list fetched successfully."));
    }

    [HttpDelete("{districtId:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int districtId)
    {
        await districtService.DeleteDistrictAsync(districtId);
        return Ok(ApiResponse.Success("District deleted successfully."));
    }
}
