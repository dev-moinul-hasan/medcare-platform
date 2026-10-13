using MedCare.Application.Abstractions.Repositories.Core;
using MedCare.Application.Abstractions.Services.Core;
using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;
using Microsoft.Extensions.Logging;
using static MedCare.Application.Exceptions.ApplicationExceptions;

namespace MedCare.Application.Services.Core;
public class OccupationService(IOccupationRepository occupationRepository, ILogger<OccupationService> logger): IOccupationService
{
    private readonly IOccupationRepository _occupationRepository = occupationRepository;
    private readonly ILogger<OccupationService> _logger = logger;

    public async Task<int> CreateOccupationAsync(OccupationReqDTO Occupation)
    {
        _logger.LogInformation("Creating Occupation: {OccupationName}", Occupation.OccupationName);

        var isDuplicate = await _occupationRepository.IsDuplicateAsync(Occupation, 0);

        if (isDuplicate)
        {
            _logger.LogWarning("Occupation creation failed - duplicate found: {OccupationName}",
                Occupation.OccupationName);
            throw new BusinessRuleException(
                "Occupation already registered in the system."
            );
        }

        var outID = await _occupationRepository.DataProcessAsync(1, Occupation, 0, userId: 1);

        if (outID <= 0)
        {
            _logger.LogError("Occupation creation failed - invalid ID returned: {OutID}", outID);
            throw new BusinessRuleException("Failed to register Occupation. Database operation did not succeed.");
        }

        _logger.LogInformation("Occupation created successfully: {OccupationId}", outID);
        return outID;
    }

    public async Task UpdateOccupationAsync(OccupationReqDTO Occupation, int occupationId)
    {
        _logger.LogInformation("Updating Occupation {OccupationId}", occupationId);

        if (occupationId <= 0)
        {
            throw new ArgumentException("Valid Occupation ID is required for update.", nameof(occupationId));
        }

        var exists = await _occupationRepository.ExistsAsync(occupationId);
        if (!exists)
        {
            _logger.LogWarning("Occupation update failed - Occupation not found: {OccupationId}", occupationId);
            throw new NotFoundException("Occupation", occupationId);
        }

        var isDuplicate = await _occupationRepository.IsDuplicateAsync(Occupation, occupationId);
        if (isDuplicate)
        {
            _logger.LogWarning("Occupation update failed - duplicate found: {OccupationId}", occupationId);
            throw new BusinessRuleException("A Occupation with this OccupationName is already registered in the system.");
        }

        var outID = await _occupationRepository.DataProcessAsync(2, Occupation, occupationId, userId: 1);
        if (outID <= 0)
        {
            _logger.LogError("Occupation update failed - invalid ID returned: {OccupationId}", occupationId);
            throw new BusinessRuleException("Failed to update Occupation. Database operation did not succeed.");
        }

        _logger.LogInformation("Occupation updated successfully: {OccupationId}", occupationId);
    }

    public async Task<OccupationReqDTO> GetOccupationByIdAsync(int occupationId)
    {
        var result = await _occupationRepository.GetByIdAsync(occupationId);
        return result ?? throw new NotFoundException("Occupation", occupationId);
    }

    public async Task<PagedResult<OccupationRespDTO>> GetOccupationByPageAsync(PageQuery query, CancellationToken ct)
    {
        return await _occupationRepository.GetByPageAsync(query, ct);
    }

    public async Task DeleteOccupationAsync(int occupationId)
    {
        _logger.LogInformation("Deleting Occupation {OccupationId} by user {UserId}", occupationId, 1);

        if (occupationId <= 0)
        {
            throw new ArgumentException("Valid Occupation ID is required for deletion.", nameof(occupationId));
        }

        var exists = await _occupationRepository.ExistsAsync(occupationId);
        if (!exists)
        {
            _logger.LogWarning("Occupation deletion failed - Occupation not found: {OccupationId}", occupationId);
            throw new NotFoundException("Occupation", occupationId);
        }

        var outID = await _occupationRepository.DataProcessAsync(3, new OccupationReqDTO { }, occupationId, userId: 1);
        if (outID <= 0)
        {
            _logger.LogError("Occupation deletion failed - invalid ID returned: {OccupationId}", occupationId);
            throw new BusinessRuleException("Failed to delete Occupation. Database operation did not succeed.");
        }

        _logger.LogInformation("Occupation deleted successfully: {OccupationId}", occupationId);
    }
}
