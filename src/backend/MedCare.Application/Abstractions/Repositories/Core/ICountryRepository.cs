using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;

namespace MedCare.Application.Abstractions.Repositories.Core;

public interface ICountryRepository
{
    Task<int> DataProcessAsync(int action, CountryReqDTO command, int countryId, int userId);
    Task<CountryReqDTO?> GetByIdAsync(int countryId);
    Task<PagedResult<CountryRespDTO>> GetByPageAsync(PageQuery query, CancellationToken ct);
    Task<bool> IsDuplicateAsync(CountryReqDTO country, int countryId);
    Task<bool> ExistsAsync(int countryId);
}
