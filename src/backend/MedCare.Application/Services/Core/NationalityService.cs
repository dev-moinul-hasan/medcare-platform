using MedCare.Application.Abstractions.Repositories.Core;
using MedCare.Application.Abstractions.Services.Core;
using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;
using Microsoft.Extensions.Logging;
using static MedCare.Application.Exceptions.ApplicationExceptions;

namespace MedCare.Application.Services.Core;
public class NationalityService(INationalityRepository nationalityRepository, ILogger<NationalityService> logger): INationalityService
{
    private readonly INationalityRepository _nationalityRepository = nationalityRepository;
    private readonly ILogger<NationalityService> _logger = logger;

    public async Task<int> CreateNationalityAsync(NationalityReqDTO nationality)
    {
        _logger.LogInformation("Creating nationality: {NationalityName}", nationality.NationalityName);

        var isDuplicate = await _nationalityRepository.IsDuplicateAsync(nationality, 0);

        if (isDuplicate)
        {
            _logger.LogWarning("Nationality creation failed - duplicate found: {NationalityName}",
                nationality.NationalityName);
            throw new BusinessRuleException(
                "Nationality already registered in the system."
            );
        }

        var outID = await _nationalityRepository.DataProcessAsync(1, nationality, 0, userId: 1);

        if (outID <= 0)
        {
            _logger.LogError("Nationality creation failed - invalid ID returned: {OutID}", outID);
            throw new BusinessRuleException("Failed to register nationality. Database operation did not succeed.");
        }

        _logger.LogInformation("Nationality created successfully: {NationalityId}", outID);
        return outID;
    }

    public async Task UpdateNationalityAsync(NationalityReqDTO nationality, int nationalityId)
    {
        _logger.LogInformation("Updating nationality {NationalityId}", nationalityId);

        if (nationalityId <= 0)
        {
            throw new ArgumentException("Valid nationality ID is required for update.", nameof(nationalityId));
        }

        var exists = await _nationalityRepository.ExistsAsync(nationalityId);
        if (!exists)
        {
            _logger.LogWarning("Nationality update failed - nationality not found: {NationalityId}", nationalityId);
            throw new NotFoundException("Nationality", nationalityId);
        }

        var isDuplicate = await _nationalityRepository.IsDuplicateAsync(nationality, nationalityId);
        if (isDuplicate)
        {
            _logger.LogWarning("Nationality update failed - duplicate found: {NationalityId}", nationalityId);
            throw new BusinessRuleException("A nationality with this NationalityName or ISOCode is already registered in the system.");
        }

        var outID = await _nationalityRepository.DataProcessAsync(2, nationality, nationalityId, userId: 1);
        if (outID <= 0)
        {
            _logger.LogError("Nationality update failed - invalid ID returned: {NationalityId}", nationalityId);
            throw new BusinessRuleException("Failed to update nationality. Database operation did not succeed.");
        }

        _logger.LogInformation("Nationality updated successfully: {NationalityId}", nationalityId);
    }

    public async Task<NationalityReqDTO> GetNationalityByIdAsync(int nationalityId)
    {
        var result = await _nationalityRepository.GetByIdAsync(nationalityId);
        return result ?? throw new NotFoundException("Nationality", nationalityId);
    }

    public async Task<PagedResult<NationalityRespDTO>> GetNationalityByPageAsync(PageQuery query, CancellationToken ct)
    {
        return await _nationalityRepository.GetByPageAsync(query, ct);
    }

    public async Task DeleteNationalityAsync(int nationalityId)
    {
        _logger.LogInformation("Deleting nationality {NationalityId} by user {UserId}", nationalityId, 1);

        if (nationalityId <= 0)
        {
            throw new ArgumentException("Valid nationality ID is required for deletion.", nameof(nationalityId));
        }

        var exists = await _nationalityRepository.ExistsAsync(nationalityId);
        if (!exists)
        {
            _logger.LogWarning("Nationality deletion failed - nationality not found: {NationalityId}", nationalityId);
            throw new NotFoundException("Nationality", nationalityId);
        }

        var outID = await _nationalityRepository.DataProcessAsync(3, new NationalityReqDTO { }, nationalityId, userId: 1);
        if (outID <= 0)
        {
            _logger.LogError("Nationality deletion failed - invalid ID returned: {NationalityId}", nationalityId);
            throw new BusinessRuleException("Failed to delete nationality. Database operation did not succeed.");
        }

        _logger.LogInformation("Nationality deleted successfully: {NationalityId}", nationalityId);
    }
}
