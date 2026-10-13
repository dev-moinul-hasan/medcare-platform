using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;

namespace MedCare.Application.Abstractions.Services.Core;
public interface IIDTypeService
{
    Task<int> CreateIDTypeAsync(IDTypeReqDTO command);
    Task UpdateIDTypeAsync(IDTypeReqDTO command, int idTypeId);
    Task<IDTypeReqDTO> GetIDTypeByIdAsync(int idTypeId);
    Task<PagedResult<IDTypeRespDTO>> GetIDTypeByPageAsync(PageQuery query, CancellationToken ct);
    Task DeleteIDTypeAsync(int idTypeId);
}
