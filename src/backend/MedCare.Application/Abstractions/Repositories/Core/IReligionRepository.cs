using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;

namespace MedCare.Application.Abstractions.Repositories.Core;

public interface IReligionRepository
{
    Task<int> DataProcessAsync(int action, ReligionReqDTO command, int religionId, int userId);
    Task<ReligionReqDTO?> GetByIdAsync(int religionId);
    Task<PagedResult<ReligionRespDTO>> GetByPageAsync(PageQuery query, CancellationToken ct);
    Task<bool> IsDuplicateAsync(ReligionReqDTO religion, int religionId);
    Task<bool> ExistsAsync(int religionId);
}
