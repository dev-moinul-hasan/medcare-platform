using MedCare.Application.Abstractions.Repositories.Core;
using MedCare.Application.Abstractions.Services.Core;
using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;
using Microsoft.Extensions.Logging;
using static MedCare.Application.Exceptions.ApplicationExceptions;

namespace MedCare.Application.Services.Core;
public class DistrictService(IDistrictRepository districtRepository, ILogger<DistrictService> logger): IDistrictService
{
    private readonly IDistrictRepository _districtRepository = districtRepository;
    private readonly ILogger<DistrictService> _logger = logger;

    public async Task<int> CreateDistrictAsync(DistrictReqDTO district)
    {
        _logger.LogInformation("Creating district: {DistrictName}", district.DistrictName);

        var isDuplicate = await _districtRepository.IsDuplicateAsync(district, 0);

        if (isDuplicate)
        {
            _logger.LogWarning("District creation failed - duplicate found: {DistrictName}",
                district.DistrictName);
            throw new BusinessRuleException(
                "District already registered in the system."
            );
        }

        var outID = await _districtRepository.DataProcessAsync(1, district, 0, userId: 1);

        if (outID <= 0)
        {
            _logger.LogError("District creation failed - invalid ID returned: {OutID}", outID);
            throw new BusinessRuleException("Failed to register district. Database operation did not succeed.");
        }

        _logger.LogInformation("District created successfully: {DistrictId}", outID);
        return outID;
    }

    public async Task UpdateDistrictAsync(DistrictReqDTO district, int districtId)
    {
        _logger.LogInformation("Updating district {DistrictId}", districtId);

        if (districtId <= 0)
        {
            throw new ArgumentException("Valid district ID is required for update.", nameof(districtId));
        }

        var exists = await _districtRepository.ExistsAsync(districtId);
        if (!exists)
        {
            _logger.LogWarning("District update failed - district not found: {DistrictId}", districtId);
            throw new NotFoundException("District", districtId);
        }

        var isDuplicate = await _districtRepository.IsDuplicateAsync(district, districtId);
        if (isDuplicate)
        {
            _logger.LogWarning("District update failed - duplicate found: {DistrictId}", districtId);
            throw new BusinessRuleException("A district with this DistrictName or ISOCode is already registered in the system.");
        }

        var outID = await _districtRepository.DataProcessAsync(2, district, districtId, userId: 1);
        if (outID <= 0)
        {
            _logger.LogError("District update failed - invalid ID returned: {DistrictId}", districtId);
            throw new BusinessRuleException("Failed to update district. Database operation did not succeed.");
        }

        _logger.LogInformation("District updated successfully: {DistrictId}", districtId);
    }

    public async Task<DistrictReqDTO> GetDistrictByIdAsync(int districtId)
    {
        var result = await _districtRepository.GetByIdAsync(districtId);
        return result ?? throw new NotFoundException("District", districtId);
    }

    public async Task<PagedResult<DistrictRespDTO>> GetDistrictByPageAsync(PageQuery query, CancellationToken ct)
    {
        return await _districtRepository.GetByPageAsync(query, ct);
    }

    public async Task DeleteDistrictAsync(int districtId)
    {
        _logger.LogInformation("Deleting district {DistrictId} by user {UserId}", districtId, 1);

        if (districtId <= 0)
        {
            throw new ArgumentException("Valid district ID is required for deletion.", nameof(districtId));
        }

        var exists = await _districtRepository.ExistsAsync(districtId);
        if (!exists)
        {
            _logger.LogWarning("District deletion failed - district not found: {DistrictId}", districtId);
            throw new NotFoundException("District", districtId);
        }

        var outID = await _districtRepository.DataProcessAsync(3, new DistrictReqDTO { }, districtId, userId: 1);
        if (outID <= 0)
        {
            _logger.LogError("District deletion failed - invalid ID returned: {DistrictId}", districtId);
            throw new BusinessRuleException("Failed to delete district. Database operation did not succeed.");
        }

        _logger.LogInformation("District deleted successfully: {DistrictId}", districtId);
    }
}
