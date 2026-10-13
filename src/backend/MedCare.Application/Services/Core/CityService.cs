using MedCare.Application.Abstractions.Repositories.Core;
using MedCare.Application.Abstractions.Services.Core;
using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;
using Microsoft.Extensions.Logging;
using static MedCare.Application.Exceptions.ApplicationExceptions;

namespace MedCare.Application.Services.Core;
public class CityService(ICityRepository cityRepository, ILogger<CityService> logger): ICityService
{
    private readonly ICityRepository _cityRepository = cityRepository;
    private readonly ILogger<CityService> _logger = logger;

    public async Task<int> CreateCityAsync(CityReqDTO City)
    {
        _logger.LogInformation("Creating City: {CityName}", City.CityName);

        var isDuplicate = await _cityRepository.IsDuplicateAsync(City, 0);

        if (isDuplicate)
        {
            _logger.LogWarning("City creation failed - duplicate found: {CityName}",
                City.CityName);
            throw new BusinessRuleException(
                "City already registered in the system."
            );
        }

        var outID = await _cityRepository.DataProcessAsync(1, City, 0, userId: 1);

        if (outID <= 0)
        {
            _logger.LogError("City creation failed - invalid ID returned: {OutID}", outID);
            throw new BusinessRuleException("Failed to register City. Database operation did not succeed.");
        }

        _logger.LogInformation("City created successfully: {CityId}", outID);
        return outID;
    }

    public async Task UpdateCityAsync(CityReqDTO City, int cityId)
    {
        _logger.LogInformation("Updating City {CityId}", cityId);

        if (cityId <= 0)
        {
            throw new ArgumentException("Valid City ID is required for update.", nameof(cityId));
        }

        var exists = await _cityRepository.ExistsAsync(cityId);
        if (!exists)
        {
            _logger.LogWarning("City update failed - City not found: {CityId}", cityId);
            throw new NotFoundException("City", cityId);
        }

        var isDuplicate = await _cityRepository.IsDuplicateAsync(City, cityId);
        if (isDuplicate)
        {
            _logger.LogWarning("City update failed - duplicate found: {CityId}", cityId);
            throw new BusinessRuleException("A City with this CityName is already registered in the system.");
        }

        var outID = await _cityRepository.DataProcessAsync(2, City, cityId, userId: 1);
        if (outID <= 0)
        {
            _logger.LogError("City update failed - invalid ID returned: {CityId}", cityId);
            throw new BusinessRuleException("Failed to update City. Database operation did not succeed.");
        }

        _logger.LogInformation("City updated successfully: {CityId}", cityId);
    }

    public async Task<CityReqDTO> GetCityByIdAsync(int cityId)
    {
        var result = await _cityRepository.GetByIdAsync(cityId);
        return result ?? throw new NotFoundException("City", cityId);
    }

    public async Task<PagedResult<CityRespDTO>> GetCityByPageAsync(PageQuery query, CancellationToken ct)
    {
        return await _cityRepository.GetByPageAsync(query, ct);
    }

    public async Task DeleteCityAsync(int cityId)
    {
        _logger.LogInformation("Deleting City {CityId} by user {UserId}", cityId, 1);

        if (cityId <= 0)
        {
            throw new ArgumentException("Valid City ID is required for deletion.", nameof(cityId));
        }

        var exists = await _cityRepository.ExistsAsync(cityId);
        if (!exists)
        {
            _logger.LogWarning("City deletion failed - City not found: {CityId}", cityId);
            throw new NotFoundException("City", cityId);
        }

        var outID = await _cityRepository.DataProcessAsync(3, new CityReqDTO { }, cityId, userId: 1);
        if (outID <= 0)
        {
            _logger.LogError("City deletion failed - invalid ID returned: {CityId}", cityId);
            throw new BusinessRuleException("Failed to delete City. Database operation did not succeed.");
        }

        _logger.LogInformation("City deleted successfully: {CityId}", cityId);
    }
}
