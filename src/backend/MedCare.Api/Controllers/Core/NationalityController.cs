using MedCare.Application.Abstractions.Services.Core;
using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace MedCare.Api.Controllers.Core;
[Route("api/[controller]")]
[ApiController]
public class NationalityController(INationalityService nationalityService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] NationalityReqDTO nationality)
    {
        var result = await nationalityService.CreateNationalityAsync(nationality);
        return CreatedAtAction(nameof(GetNationalityById), new { nationalityId = result },
            ApiResponse<int>.Created(result, "Nationality registered successfully."));
    }

    [HttpPut("{nationalityId:int}")]
    [ProducesResponseType(typeof(ApiResponse<NationalityReqDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int nationalityId, [FromBody] NationalityReqDTO nationality)
    {
        await nationalityService.UpdateNationalityAsync(nationality, nationalityId);
        var updatedNationality = await nationalityService.GetNationalityByIdAsync(nationalityId);
        return Ok(ApiResponse<NationalityReqDTO>.Success(updatedNationality, "Nationality updated successfully."));
    }

    [HttpGet("{nationalityId:int}")]
    [ProducesResponseType(typeof(ApiResponse<NationalityReqDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetNationalityById(int nationalityId)
    {
        var result = await nationalityService.GetNationalityByIdAsync(nationalityId);
        return Ok(ApiResponse<NationalityReqDTO>.Success(result, "Nationality fetched successfully."));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<NationalityReqDTO>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PagedResult<NationalityRespDTO>>>> GetNationalityByPage([FromQuery, StringLength(100)] string? search, [FromQuery, StringLength(20)] string? sortBy, [FromQuery, RegularExpression("(?i)^(asc|desc)$")] string? sortDirection, [FromQuery, Range(1, int.MaxValue)] int page = 1, [FromQuery, Range(1, 100)] int pageSize = 20, CancellationToken ct = default)
    {
        var query = new PageQuery(search, sortBy, sortDirection, page, pageSize);
        var result = await nationalityService.GetNationalityByPageAsync(query, ct);

        return Ok(ApiResponse<PagedResult<NationalityRespDTO>>.Success(result, "Nationality list fetched successfully."));
    }

    [HttpDelete("{nationalityId:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int nationalityId)
    {
        await nationalityService.DeleteNationalityAsync(nationalityId);
        return Ok(ApiResponse.Success("Nationality deleted successfully."));
    }
}
