using MedCare.Application.Abstractions.Repositories;
using MedCare.Application.Abstractions.Services;
using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests;
using Microsoft.Extensions.Logging;
using static MedCare.Application.Exceptions.ApplicationExceptions;

namespace MedCare.Application.Services
{
    internal class PatientService(IPatientRepository patientRepository, ILogger<PatientService> logger) : IPatientService
    {
        private readonly IPatientRepository _patientRepository = patientRepository;
        private readonly ILogger<PatientService> _logger = logger;

        public async Task<int> CreatePatientAsync(PatientReqDTO patient)
        {
            _logger.LogInformation("Creating patient: {FirstName} {LastName}", patient.FirstName, patient.LastName);

            var isDuplicate = await _patientRepository.IsDuplicateAsync(patient);

            if (isDuplicate)
            {
                _logger.LogWarning("Patient creation failed - duplicate found: {Mobile}/{NationalId}",
                    patient.MobileNo, patient.NationalId);
                throw new BusinessRuleException(
                    "A patient with this mobile number or national ID is already registered in the system."
                );
            }

            // Note: userId should ideally come from HttpContext/Claims, not hardcoded
            var outID = await _patientRepository.DataProcessAsync(1, patient, 0, userId: 1);

            if (outID <= 0)
            {
                _logger.LogError("Patient creation failed - invalid ID returned: {OutID}", outID);
                throw new BusinessRuleException("Failed to register patient. Database operation did not succeed.");
            }

            _logger.LogInformation("Patient created successfully: {PatientId}", outID);
            return outID;
        }

        public async Task UpdatePatientAsync(PatientReqDTO patient, int patientId)
        {
            _logger.LogInformation("Updating patient {PatientId}", patientId);

            if (patientId <= 0)
            {
                throw new ArgumentException("Valid patient ID is required for update.", nameof(patientId));
            }

            var exists = await _patientRepository.ExistsAsync(patientId);
            if (!exists)
            {
                _logger.LogWarning("Patient update failed - patient not found: {PatientId}", patientId);
                throw new NotFoundException("Patient", patientId);
            }

            var isDuplicate = await _patientRepository.IsDuplicateAsync(patient);
            if (isDuplicate)
            {
                _logger.LogWarning("Patient update failed - duplicate found: {PatientId}", patientId);
                throw new BusinessRuleException("A patient with this mobile number is already registered in the system.");
            }

            var outID = await _patientRepository.DataProcessAsync(2, patient, patientId, userId: 1);
            if (outID <= 0)
            {
                _logger.LogError("Patient update failed - invalid ID returned: {PatientId}", patientId);
                throw new BusinessRuleException("Failed to update patient. Database operation did not succeed.");
            }

            _logger.LogInformation("Patient updated successfully: {PatientId}", patientId);
        }

        public async Task<PatientReqDTO> GetPatientByIdAsync(int patientId)
        {
            var result = await _patientRepository.GetPatientByIdAsync(patientId);
            return result ?? throw new NotFoundException("Patient", patientId);
        }

        public async Task<PagedResult<PatientReqDTO>> GetPatientByPageAsync(PageQuery query, CancellationToken ct)
        {
            return await _patientRepository.GetPatientByPageAsync(query, ct);
        }

        public async Task DeletePatientAsync(int patientId, int userId)
        {
            _logger.LogInformation("Deleting patient {PatientId} by user {UserId}", patientId, userId);

            if (patientId <= 0)
            {
                throw new ArgumentException("Valid patient ID is required for deletion.", nameof(patientId));
            }

            var exists = await _patientRepository.ExistsAsync(patientId);
            if (!exists)
            {
                _logger.LogWarning("Patient deletion failed - patient not found: {PatientId}", patientId);
                throw new NotFoundException("Patient", patientId);
            }

            var outID = await _patientRepository.DataProcessAsync(3, new PatientReqDTO { }, patientId, userId);
            if (outID <= 0)
            {
                _logger.LogError("Patient deletion failed - invalid ID returned: {PatientId}", patientId);
                throw new BusinessRuleException("Failed to delete patient. Database operation did not succeed.");
            }

            _logger.LogInformation("Patient deleted successfully: {PatientId}", patientId);
        }
    }
}