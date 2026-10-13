using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;

namespace MedCare.Application.Abstractions.Services.Core;
public interface IReligionService
{
    Task<int> CreateReligionAsync(ReligionReqDTO command);
    Task UpdateReligionAsync(ReligionReqDTO command, int religionId);
    Task<ReligionReqDTO> GetReligionByIdAsync(int religionId);
    Task<PagedResult<ReligionRespDTO>> GetReligionByPageAsync(PageQuery query, CancellationToken ct);
    Task DeleteReligionAsync(int religionId);
}
