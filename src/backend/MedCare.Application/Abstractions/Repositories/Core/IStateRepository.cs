using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;

namespace MedCare.Application.Abstractions.Repositories.Core;

public interface IStateRepository
{
    Task<int> DataProcessAsync(int action, StateReqDTO command, int countryId, int userId);
    Task<StateReqDTO?> GetByIdAsync(int countryId);
    Task<PagedResult<StateRespDTO>> GetByPageAsync(PageQuery query, CancellationToken ct);
    Task<bool> IsDuplicateAsync(StateReqDTO country, int countryId);
    Task<bool> ExistsAsync(int countryId);
}
