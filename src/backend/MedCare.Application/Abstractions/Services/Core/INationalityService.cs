using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;

namespace MedCare.Application.Abstractions.Services.Core;
public interface INationalityService
{
    Task<int> CreateNationalityAsync(NationalityReqDTO command);
    Task UpdateNationalityAsync(NationalityReqDTO command, int nationalityId);
    Task<NationalityReqDTO> GetNationalityByIdAsync(int nationalityId);
    Task<PagedResult<NationalityRespDTO>> GetNationalityByPageAsync(PageQuery query, CancellationToken ct);
    Task DeleteNationalityAsync(int nationalityId);
}
