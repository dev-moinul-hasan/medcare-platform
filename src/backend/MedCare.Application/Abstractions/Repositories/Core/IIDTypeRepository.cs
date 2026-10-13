using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;

namespace MedCare.Application.Abstractions.Repositories.Core;

public interface IIDTypeRepository
{
    Task<int> DataProcessAsync(int action, IDTypeReqDTO command, int idTypeId, int userId);
    Task<IDTypeReqDTO?> GetByIdAsync(int idTypeId);
    Task<PagedResult<IDTypeRespDTO>> GetByPageAsync(PageQuery query, CancellationToken ct);
    Task<bool> IsDuplicateAsync(IDTypeReqDTO idType, int idTypeId);
    Task<bool> ExistsAsync(int idTypeId);
}
