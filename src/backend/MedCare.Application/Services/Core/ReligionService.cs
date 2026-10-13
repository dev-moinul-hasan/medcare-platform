using MedCare.Application.Abstractions.Repositories.Core;
using MedCare.Application.Abstractions.Services.Core;
using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;
using Microsoft.Extensions.Logging;
using static MedCare.Application.Exceptions.ApplicationExceptions;

namespace MedCare.Application.Services.Core;
public class ReligionService(IReligionRepository religionRepository, ILogger<ReligionService> logger): IReligionService
{
    private readonly IReligionRepository _religionRepository = religionRepository;
    private readonly ILogger<ReligionService> _logger = logger;

    public async Task<int> CreateReligionAsync(ReligionReqDTO Religion)
    {
        _logger.LogInformation("Creating Religion: {ReligionName}", Religion.ReligionName);

        var isDuplicate = await _religionRepository.IsDuplicateAsync(Religion, 0);

        if (isDuplicate)
        {
            _logger.LogWarning("Religion creation failed - duplicate found: {ReligionName}",
                Religion.ReligionName);
            throw new BusinessRuleException(
                "Religion already registered in the system."
            );
        }

        var outID = await _religionRepository.DataProcessAsync(1, Religion, 0, userId: 1);

        if (outID <= 0)
        {
            _logger.LogError("Religion creation failed - invalid ID returned: {OutID}", outID);
            throw new BusinessRuleException("Failed to register Religion. Database operation did not succeed.");
        }

        _logger.LogInformation("Religion created successfully: {ReligionId}", outID);
        return outID;
    }

    public async Task UpdateReligionAsync(ReligionReqDTO Religion, int religionId)
    {
        _logger.LogInformation("Updating Religion {ReligionId}", religionId);

        if (religionId <= 0)
        {
            throw new ArgumentException("Valid Religion ID is required for update.", nameof(religionId));
        }

        var exists = await _religionRepository.ExistsAsync(religionId);
        if (!exists)
        {
            _logger.LogWarning("Religion update failed - Religion not found: {ReligionId}", religionId);
            throw new NotFoundException("Religion", religionId);
        }

        var isDuplicate = await _religionRepository.IsDuplicateAsync(Religion, religionId);
        if (isDuplicate)
        {
            _logger.LogWarning("Religion update failed - duplicate found: {ReligionId}", religionId);
            throw new BusinessRuleException("A Religion with this ReligionName is already registered in the system.");
        }

        var outID = await _religionRepository.DataProcessAsync(2, Religion, religionId, userId: 1);
        if (outID <= 0)
        {
            _logger.LogError("Religion update failed - invalid ID returned: {ReligionId}", religionId);
            throw new BusinessRuleException("Failed to update Religion. Database operation did not succeed.");
        }

        _logger.LogInformation("Religion updated successfully: {ReligionId}", religionId);
    }

    public async Task<ReligionReqDTO> GetReligionByIdAsync(int religionId)
    {
        var result = await _religionRepository.GetByIdAsync(religionId);
        return result ?? throw new NotFoundException("Religion", religionId);
    }

    public async Task<PagedResult<ReligionRespDTO>> GetReligionByPageAsync(PageQuery query, CancellationToken ct)
    {
        return await _religionRepository.GetByPageAsync(query, ct);
    }

    public async Task DeleteReligionAsync(int religionId)
    {
        _logger.LogInformation("Deleting Religion {ReligionId} by user {UserId}", religionId, 1);

        if (religionId <= 0)
        {
            throw new ArgumentException("Valid Religion ID is required for deletion.", nameof(religionId));
        }

        var exists = await _religionRepository.ExistsAsync(religionId);
        if (!exists)
        {
            _logger.LogWarning("Religion deletion failed - Religion not found: {ReligionId}", religionId);
            throw new NotFoundException("Religion", religionId);
        }

        var outID = await _religionRepository.DataProcessAsync(3, new ReligionReqDTO { }, religionId, userId: 1);
        if (outID <= 0)
        {
            _logger.LogError("Religion deletion failed - invalid ID returned: {ReligionId}", religionId);
            throw new BusinessRuleException("Failed to delete Religion. Database operation did not succeed.");
        }

        _logger.LogInformation("Religion deleted successfully: {ReligionId}", religionId);
    }
}
