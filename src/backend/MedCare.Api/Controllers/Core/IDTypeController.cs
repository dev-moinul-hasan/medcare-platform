using MedCare.Application.Abstractions.Services.Core;
using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace MedCare.Api.Controllers.Core;
[Route("api/[controller]")]
[ApiController]
public class IDTypeController(IIDTypeService idTypeService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] IDTypeReqDTO idType)
    {
        var result = await idTypeService.CreateIDTypeAsync(idType);
        return CreatedAtAction(nameof(GetIDTypeById), new { idTypeId = result },
            ApiResponse<int>.Created(result, "IDType registered successfully."));
    }

    [HttpPut("{idTypeId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IDTypeReqDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int idTypeId, [FromBody] IDTypeReqDTO idType)
    {
        await idTypeService.UpdateIDTypeAsync(idType, idTypeId);
        var updatedIDType = await idTypeService.GetIDTypeByIdAsync(idTypeId);
        return Ok(ApiResponse<IDTypeReqDTO>.Success(updatedIDType, "IDType updated successfully."));
    }

    [HttpGet("{idTypeId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IDTypeReqDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetIDTypeById(int idTypeId)
    {
        var result = await idTypeService.GetIDTypeByIdAsync(idTypeId);
        return Ok(ApiResponse<IDTypeReqDTO>.Success(result, "IDType fetched successfully."));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<IDTypeReqDTO>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PagedResult<IDTypeRespDTO>>>> GetIDTypeByPage([FromQuery, StringLength(100)] string? search, [FromQuery, StringLength(20)] string? sortBy, [FromQuery, RegularExpression("(?i)^(asc|desc)$")] string? sortDirection, [FromQuery, Range(1, int.MaxValue)] int page = 1, [FromQuery, Range(1, 100)] int pageSize = 20, CancellationToken ct = default)
    {
        var query = new PageQuery(search, sortBy, sortDirection, page, pageSize);
        var result = await idTypeService.GetIDTypeByPageAsync(query, ct);

        return Ok(ApiResponse<PagedResult<IDTypeRespDTO>>.Success(result, "IDType list fetched successfully."));
    }

    [HttpDelete("{idTypeId:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int idTypeId)
    {
        await idTypeService.DeleteIDTypeAsync(idTypeId);
        return Ok(ApiResponse.Success("IDType deleted successfully."));
    }
}
