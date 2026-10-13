using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;

namespace MedCare.Application.Abstractions.Repositories.Core;

public interface INationalityRepository
{
    Task<int> DataProcessAsync(int action, NationalityReqDTO command, int nationalityId, int userId);
    Task<NationalityReqDTO?> GetByIdAsync(int nationalityId);
    Task<PagedResult<NationalityRespDTO>> GetByPageAsync(PageQuery query, CancellationToken ct);
    Task<bool> IsDuplicateAsync(NationalityReqDTO nationality, int nationalityId);
    Task<bool> ExistsAsync(int nationalityId);
}
