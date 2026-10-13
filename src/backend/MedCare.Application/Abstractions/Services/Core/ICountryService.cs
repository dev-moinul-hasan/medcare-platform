using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;

namespace MedCare.Application.Abstractions.Services.Core;
public interface ICountryService
{
    Task<int> CreateCountryAsync(CountryReqDTO command);
    Task UpdateCountryAsync(CountryReqDTO command, int countryId);
    Task<CountryReqDTO> GetCountryByIdAsync(int countryId);
    Task<PagedResult<CountryRespDTO>> GetCountryByPageAsync(PageQuery query, CancellationToken ct);
    Task DeleteCountryAsync(int countryId);
}
