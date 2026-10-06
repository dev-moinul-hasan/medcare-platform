using MedCare.Application.Abstractions.Services;
using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace MedCare.Api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PatientsController(IPatientService patientService) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] PatientReqDTO patient)
        {
            var result = await patientService.CreatePatientAsync(patient);
            return CreatedAtAction(nameof(GetPatientById), new { patientId = result },
                ApiResponse<int>.Created(result, "Patient registered successfully."));
        }

        [HttpPut("{patientId:int}")]
        [ProducesResponseType(typeof(ApiResponse<PatientReqDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int patientId, [FromBody] PatientReqDTO patient)
        {
            await patientService.UpdatePatientAsync(patient, patientId);
            var updatedPatient = await patientService.GetPatientByIdAsync(patientId);
            return Ok(ApiResponse<PatientReqDTO>.Success(updatedPatient, "Patient updated successfully."));
        }

        [HttpGet("{patientId:int}")]
        [ProducesResponseType(typeof(ApiResponse<PatientReqDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPatientById(int patientId)
        {
            var result = await patientService.GetPatientByIdAsync(patientId);
            return Ok(ApiResponse<PatientReqDTO>.Success(result, "Patient fetched successfully."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<PatientReqDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PagedResult<PatientReqDTO>>>> GetPatientByPage([FromQuery, StringLength(100)] string? search, [FromQuery, StringLength(20)] string? sortBy, [FromQuery, RegularExpression("(?i)^(asc|desc)$")] string? sortDirection, [FromQuery, Range(1, int.MaxValue)] int page = 1, [FromQuery, Range(1, 100)] int pageSize = 20, CancellationToken ct = default)
        {
            var query = new PageQuery(search, sortBy, sortDirection, page, pageSize);
            var result = await patientService.GetPatientByPageAsync(query, ct);

            return Ok(ApiResponse<PagedResult<PatientReqDTO>>.Success(result, "Patient list fetched successfully."));
        }

        [HttpDelete("{patientId:int}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int patientId, [FromQuery] int userId)
        {
            await patientService.DeletePatientAsync(patientId, userId);
            return Ok(ApiResponse.Success("Patient deleted successfully."));
        }
    }
}