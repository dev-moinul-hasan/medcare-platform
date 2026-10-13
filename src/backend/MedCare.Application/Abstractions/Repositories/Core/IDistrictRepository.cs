using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;

namespace MedCare.Application.Abstractions.Repositories.Core;

public interface IDistrictRepository
{
    Task<int> DataProcessAsync(int action, DistrictReqDTO command, int countryId, int userId);
    Task<DistrictReqDTO?> GetByIdAsync(int countryId);
    Task<PagedResult<DistrictRespDTO>> GetByPageAsync(PageQuery query, CancellationToken ct);
    Task<bool> IsDuplicateAsync(DistrictReqDTO country, int countryId);
    Task<bool> ExistsAsync(int countryId);
}
