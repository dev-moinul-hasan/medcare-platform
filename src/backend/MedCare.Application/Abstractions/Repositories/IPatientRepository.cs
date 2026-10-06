using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests;

namespace MedCare.Application.Abstractions.Repositories;

public interface IPatientRepository
{
    Task<int> DataProcessAsync(int action, PatientReqDTO command, int patientId, int userId);
    Task<PatientReqDTO?> GetPatientByIdAsync(int patientId);
    Task<PagedResult<PatientReqDTO>> GetPatientByPageAsync(PageQuery query, CancellationToken ct);
    Task<bool> IsDuplicateAsync(PatientReqDTO patient);
    Task<bool> ExistsAsync(int patientId);
}