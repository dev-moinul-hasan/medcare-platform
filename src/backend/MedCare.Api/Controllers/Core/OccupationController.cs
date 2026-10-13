using MedCare.Application.Abstractions.Services.Core;
using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace MedCare.Api.Controllers.Core;
[Route("api/[controller]")]
[ApiController]
public class OccupationController(IOccupationService occupationService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] OccupationReqDTO occupation)
    {
        var result = await occupationService.CreateOccupationAsync(occupation);
        return CreatedAtAction(nameof(GetOccupationById), new { occupationId = result },
            ApiResponse<int>.Created(result, "Occupation registered successfully."));
    }

    [HttpPut("{occupationId:int}")]
    [ProducesResponseType(typeof(ApiResponse<OccupationReqDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int occupationId, [FromBody] OccupationReqDTO occupation)
    {
        await occupationService.UpdateOccupationAsync(occupation, occupationId);
        var updatedOccupation = await occupationService.GetOccupationByIdAsync(occupationId);
        return Ok(ApiResponse<OccupationReqDTO>.Success(updatedOccupation, "Occupation updated successfully."));
    }

    [HttpGet("{occupationId:int}")]
    [ProducesResponseType(typeof(ApiResponse<OccupationReqDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOccupationById(int occupationId)
    {
        var result = await occupationService.GetOccupationByIdAsync(occupationId);
        return Ok(ApiResponse<OccupationReqDTO>.Success(result, "Occupation fetched successfully."));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<OccupationReqDTO>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PagedResult<OccupationRespDTO>>>> GetOccupationByPage([FromQuery, StringLength(100)] string? search, [FromQuery, StringLength(20)] string? sortBy, [FromQuery, RegularExpression("(?i)^(asc|desc)$")] string? sortDirection, [FromQuery, Range(1, int.MaxValue)] int page = 1, [FromQuery, Range(1, 100)] int pageSize = 20, CancellationToken ct = default)
    {
        var query = new PageQuery(search, sortBy, sortDirection, page, pageSize);
        var result = await occupationService.GetOccupationByPageAsync(query, ct);

        return Ok(ApiResponse<PagedResult<OccupationRespDTO>>.Success(result, "Occupation list fetched successfully."));
    }

    [HttpDelete("{occupationId:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int occupationId)
    {
        await occupationService.DeleteOccupationAsync(occupationId);
        return Ok(ApiResponse.Success("Occupation deleted successfully."));
    }
}
