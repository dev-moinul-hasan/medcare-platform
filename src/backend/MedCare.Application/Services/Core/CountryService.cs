using MedCare.Application.Abstractions.Repositories.Core;
using MedCare.Application.Abstractions.Services.Core;
using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;
using Microsoft.Extensions.Logging;
using static MedCare.Application.Exceptions.ApplicationExceptions;

namespace MedCare.Application.Services.Core;
public class CountryService(ICountryRepository countryRepository, ILogger<CountryService> logger): ICountryService
{
    private readonly ICountryRepository _countryRepository = countryRepository;
    private readonly ILogger<CountryService> _logger = logger;

    public async Task<int> CreateCountryAsync(CountryReqDTO country)
    {
        _logger.LogInformation("Creating country: {CountryName}", country.CountryName);

        var isDuplicate = await _countryRepository.IsDuplicateAsync(country, 0);

        if (isDuplicate)
        {
            _logger.LogWarning("Country creation failed - duplicate found: {CountryName}",
                country.CountryName);
            throw new BusinessRuleException(
                "Country already registered in the system."
            );
        }

        var outID = await _countryRepository.DataProcessAsync(1, country, 0, userId: 1);

        if (outID <= 0)
        {
            _logger.LogError("Country creation failed - invalid ID returned: {OutID}", outID);
            throw new BusinessRuleException("Failed to register country. Database operation did not succeed.");
        }

        _logger.LogInformation("Country created successfully: {CountryId}", outID);
        return outID;
    }

    public async Task UpdateCountryAsync(CountryReqDTO country, int countryId)
    {
        _logger.LogInformation("Updating country {CountryId}", countryId);

        if (countryId <= 0)
        {
            throw new ArgumentException("Valid country ID is required for update.", nameof(countryId));
        }

        var exists = await _countryRepository.ExistsAsync(countryId);
        if (!exists)
        {
            _logger.LogWarning("Country update failed - country not found: {CountryId}", countryId);
            throw new NotFoundException("Country", countryId);
        }

        var isDuplicate = await _countryRepository.IsDuplicateAsync(country, countryId);
        if (isDuplicate)
        {
            _logger.LogWarning("Country update failed - duplicate found: {CountryId}", countryId);
            throw new BusinessRuleException("A country with this CountryName or ISOCode is already registered in the system.");
        }

        var outID = await _countryRepository.DataProcessAsync(2, country, countryId, userId: 1);
        if (outID <= 0)
        {
            _logger.LogError("Country update failed - invalid ID returned: {CountryId}", countryId);
            throw new BusinessRuleException("Failed to update country. Database operation did not succeed.");
        }

        _logger.LogInformation("Country updated successfully: {CountryId}", countryId);
    }

    public async Task<CountryReqDTO> GetCountryByIdAsync(int countryId)
    {
        var result = await _countryRepository.GetByIdAsync(countryId);
        return result ?? throw new NotFoundException("Country", countryId);
    }

    public async Task<PagedResult<CountryRespDTO>> GetCountryByPageAsync(PageQuery query, CancellationToken ct)
    {
        return await _countryRepository.GetByPageAsync(query, ct);
    }

    public async Task DeleteCountryAsync(int countryId)
    {
        _logger.LogInformation("Deleting country {CountryId} by user {UserId}", countryId, 1);

        if (countryId <= 0)
        {
            throw new ArgumentException("Valid country ID is required for deletion.", nameof(countryId));
        }

        var exists = await _countryRepository.ExistsAsync(countryId);
        if (!exists)
        {
            _logger.LogWarning("Country deletion failed - country not found: {CountryId}", countryId);
            throw new NotFoundException("Country", countryId);
        }

        var outID = await _countryRepository.DataProcessAsync(3, new CountryReqDTO { }, countryId, userId: 1);
        if (outID <= 0)
        {
            _logger.LogError("Country deletion failed - invalid ID returned: {CountryId}", countryId);
            throw new BusinessRuleException("Failed to delete country. Database operation did not succeed.");
        }

        _logger.LogInformation("Country deleted successfully: {CountryId}", countryId);
    }
}
