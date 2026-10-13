using MedCare.Application.Abstractions.Services.Core;
using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace MedCare.Api.Controllers.Core;
[Route("api/[controller]")]
[ApiController]
public class StateController(IStateService stateService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] StateReqDTO State)
    {
        var result = await stateService.CreateStateAsync(State);
        return CreatedAtAction(nameof(GetStateById), new { StateId = result },
            ApiResponse<int>.Created(result, "State registered successfully."));
    }

    [HttpPut("{StateId:int}")]
    [ProducesResponseType(typeof(ApiResponse<StateReqDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int StateId, [FromBody] StateReqDTO State)
    {
        await stateService.UpdateStateAsync(State, StateId);
        var updatedState = await stateService.GetStateByIdAsync(StateId);
        return Ok(ApiResponse<StateReqDTO>.Success(updatedState, "State updated successfully."));
    }

    [HttpGet("{StateId:int}")]
    [ProducesResponseType(typeof(ApiResponse<StateReqDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetStateById(int StateId)
    {
        var result = await stateService.GetStateByIdAsync(StateId);
        return Ok(ApiResponse<StateReqDTO>.Success(result, "State fetched successfully."));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<StateReqDTO>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PagedResult<StateRespDTO>>>> GetStateByPage([FromQuery, StringLength(100)] string? search, [FromQuery, StringLength(20)] string? sortBy, [FromQuery, RegularExpression("(?i)^(asc|desc)$")] string? sortDirection, [FromQuery, Range(1, int.MaxValue)] int page = 1, [FromQuery, Range(1, 100)] int pageSize = 20, CancellationToken ct = default)
    {
        var query = new PageQuery(search, sortBy, sortDirection, page, pageSize);
        var result = await stateService.GetStateByPageAsync(query, ct);

        return Ok(ApiResponse<PagedResult<StateRespDTO>>.Success(result, "State list fetched successfully."));
    }

    [HttpDelete("{StateId:int}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int StateId)
    {
        await stateService.DeleteStateAsync(StateId);
        return Ok(ApiResponse.Success("State deleted successfully."));
    }
}
