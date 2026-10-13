using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;

namespace MedCare.Application.Abstractions.Services.Core;
public interface ICityService
{
    Task<int> CreateCityAsync(CityReqDTO command);
    Task UpdateCityAsync(CityReqDTO command, int cityId);
    Task<CityReqDTO> GetCityByIdAsync(int cityId);
    Task<PagedResult<CityRespDTO>> GetCityByPageAsync(PageQuery query, CancellationToken ct);
    Task DeleteCityAsync(int cityId);
}
