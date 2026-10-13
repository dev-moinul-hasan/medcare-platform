using MedCare.Application.Abstractions.Services.Core;
using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace MedCare.Api.Controllers.Core;
[Route("api/[controller]")]
[ApiController]
public class ReligionController(IReligionService religionService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] ReligionReqDTO religion)
    {
        var result = await religionService.CreateReligionAsync(religion);
        return CreatedAtAction(nameof(GetReligionById), new { religionId = result },
            ApiResponse<int>.Created(result, "Religion registered successfully."));
    }

    [HttpPut("{religionId:int}")]
    [ProducesResponseType(typeof(ApiResponse<ReligionReqDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int religionId, [FromBody] ReligionReqDTO religion)
    {
        await religionService.UpdateReligionAsync(religion, religionId);
        var updatedReligion = await religionService.GetReligionByIdAsync(religionId);
        return Ok(ApiResponse<ReligionReqDTO>.Success(updatedReligion, "Religion updated successfully."));
    }

    [HttpGet("{religionId:int}")]
    [ProducesResponseType(typeof(ApiResponse<ReligionReqDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetReligionById(int religionId)
    {
        var result = await religionService.GetReligionByIdAsync(religionId);
        return Ok(ApiResponse<ReligionReqDTO>.Success(result, "Religion fetched successfully."));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ReligionReqDTO>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PagedResult<ReligionRespDTO>>>> GetReligionByPage([FromQuery, StringLength(100)] string? search, [FromQuery, StringLength(20)] string? sortBy, [FromQuery, RegularExpression("(?i)^(asc|desc)$")] string? sortDirection, [FromQuery, Range(1, int.MaxValue)] int page = 1, [FromQuery, Range(1, 100)] int pageSize = 20, CancellationToken ct = default)
    {
        var query = new PageQuery(search, sortBy, sortDirection, page, pageSize);
        var result = await religionService.GetReligionByPageAsync(query, ct);

        return Ok(ApiResponse<PagedResult<ReligionRespDTO>>.Success(result, "Religion list fetched successfully."));
    }

    [HttpDelete("{religionId:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int religionId)
    {
        await religionService.DeleteReligionAsync(religionId);
        return Ok(ApiResponse.Success("Religion deleted successfully."));
    }
}
