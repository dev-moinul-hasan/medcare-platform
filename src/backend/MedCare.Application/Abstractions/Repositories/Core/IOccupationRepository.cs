using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;

namespace MedCare.Application.Abstractions.Repositories.Core;

public interface IOccupationRepository
{
    Task<int> DataProcessAsync(int action, OccupationReqDTO command, int occupationId, int userId);
    Task<OccupationReqDTO?> GetByIdAsync(int occupationId);
    Task<PagedResult<OccupationRespDTO>> GetByPageAsync(PageQuery query, CancellationToken ct);
    Task<bool> IsDuplicateAsync(OccupationReqDTO occupation, int occupationId);
    Task<bool> ExistsAsync(int occupationId);
}
