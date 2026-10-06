using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests;

namespace MedCare.Application.Abstractions.Services;

public interface IPatientService
{
    Task<int> CreatePatientAsync(PatientReqDTO command);
    Task UpdatePatientAsync(PatientReqDTO command, int patientId);
    Task<PatientReqDTO> GetPatientByIdAsync(int patientId);
    Task<PagedResult<PatientReqDTO>> GetPatientByPageAsync(PageQuery query, CancellationToken ct);
    Task DeletePatientAsync(int patientId, int userId);
}