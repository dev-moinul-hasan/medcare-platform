using MedCare.Application.Abstractions.Repositories.Core;
using MedCare.Application.Abstractions.Services.Core;
using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;
using Microsoft.Extensions.Logging;
using static MedCare.Application.Exceptions.ApplicationExceptions;

namespace MedCare.Application.Services.Core;
public class StateService(IStateRepository stateRepository, ILogger<StateService> logger): IStateService
{
    private readonly IStateRepository _stateRepository = stateRepository;
    private readonly ILogger<StateService> _logger = logger;

    public async Task<int> CreateStateAsync(StateReqDTO state)
    {
        _logger.LogInformation("Creating state: {StateName}", state.StateName);

        var isDuplicate = await _stateRepository.IsDuplicateAsync(state, 0);

        if (isDuplicate)
        {
            _logger.LogWarning("State creation failed - duplicate found: {StateName}",
                state.StateName);
            throw new BusinessRuleException(
                "State already registered in the system."
            );
        }

        var outID = await _stateRepository.DataProcessAsync(1, state, 0, userId: 1);

        if (outID <= 0)
        {
            _logger.LogError("State creation failed - invalid ID returned: {OutID}", outID);
            throw new BusinessRuleException("Failed to register state. Database operation did not succeed.");
        }

        _logger.LogInformation("State created successfully: {StateId}", outID);
        return outID;
    }

    public async Task UpdateStateAsync(StateReqDTO state, int stateId)
    {
        _logger.LogInformation("Updating state {StateId}", stateId);

        if (stateId <= 0)
        {
            throw new ArgumentException("Valid state ID is required for update.", nameof(stateId));
        }

        var exists = await _stateRepository.ExistsAsync(stateId);
        if (!exists)
        {
            _logger.LogWarning("State update failed - state not found: {StateId}", stateId);
            throw new NotFoundException("State", stateId);
        }

        var isDuplicate = await _stateRepository.IsDuplicateAsync(state, stateId);
        if (isDuplicate)
        {
            _logger.LogWarning("State update failed - duplicate found: {StateId}", stateId);
            throw new BusinessRuleException("A state with this StateName or ISOCode is already registered in the system.");
        }

        var outID = await _stateRepository.DataProcessAsync(2, state, stateId, userId: 1);
        if (outID <= 0)
        {
            _logger.LogError("State update failed - invalid ID returned: {StateId}", stateId);
            throw new BusinessRuleException("Failed to update state. Database operation did not succeed.");
        }

        _logger.LogInformation("State updated successfully: {StateId}", stateId);
    }

    public async Task<StateReqDTO> GetStateByIdAsync(int stateId)
    {
        var result = await _stateRepository.GetByIdAsync(stateId);
        return result ?? throw new NotFoundException("State", stateId);
    }

    public async Task<PagedResult<StateRespDTO>> GetStateByPageAsync(PageQuery query, CancellationToken ct)
    {
        return await _stateRepository.GetByPageAsync(query, ct);
    }

    public async Task DeleteStateAsync(int stateId)
    {
        _logger.LogInformation("Deleting state {StateId} by user {UserId}", stateId, 1);

        if (stateId <= 0)
        {
            throw new ArgumentException("Valid state ID is required for deletion.", nameof(stateId));
        }

        var exists = await _stateRepository.ExistsAsync(stateId);
        if (!exists)
        {
            _logger.LogWarning("State deletion failed - state not found: {StateId}", stateId);
            throw new NotFoundException("State", stateId);
        }

        var outID = await _stateRepository.DataProcessAsync(3, new StateReqDTO { }, stateId, userId: 1);
        if (outID <= 0)
        {
            _logger.LogError("State deletion failed - invalid ID returned: {StateId}", stateId);
            throw new BusinessRuleException("Failed to delete state. Database operation did not succeed.");
        }

        _logger.LogInformation("State deleted successfully: {StateId}", stateId);
    }
}
