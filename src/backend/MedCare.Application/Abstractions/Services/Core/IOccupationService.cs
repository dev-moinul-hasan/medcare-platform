using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;

namespace MedCare.Application.Abstractions.Services.Core;
public interface IOccupationService
{
    Task<int> CreateOccupationAsync(OccupationReqDTO command);
    Task UpdateOccupationAsync(OccupationReqDTO command, int occupationId);
    Task<OccupationReqDTO> GetOccupationByIdAsync(int occupationId);
    Task<PagedResult<OccupationRespDTO>> GetOccupationByPageAsync(PageQuery query, CancellationToken ct);
    Task DeleteOccupationAsync(int occupationId);
}
