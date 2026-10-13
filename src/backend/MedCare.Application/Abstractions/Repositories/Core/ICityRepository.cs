using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;

namespace MedCare.Application.Abstractions.Repositories.Core;

public interface ICityRepository
{
    Task<int> DataProcessAsync(int action, CityReqDTO command, int cityId, int userId);
    Task<CityReqDTO?> GetByIdAsync(int cityId);
    Task<PagedResult<CityRespDTO>> GetByPageAsync(PageQuery query, CancellationToken ct);
    Task<bool> IsDuplicateAsync(CityReqDTO city, int cityId);
    Task<bool> ExistsAsync(int cityId);
}
