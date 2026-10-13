using MedCare.Application.Abstractions.Repositories.Core;
using MedCare.Application.Abstractions.Services.Core;
using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;
using Microsoft.Extensions.Logging;
using static MedCare.Application.Exceptions.ApplicationExceptions;

namespace MedCare.Application.Services.Core;
public class IDTypeService(IIDTypeRepository idTypeRepository, ILogger<IDTypeService> logger): IIDTypeService
{
    private readonly IIDTypeRepository _idTypeRepository = idTypeRepository;
    private readonly ILogger<IDTypeService> _logger = logger;

    public async Task<int> CreateIDTypeAsync(IDTypeReqDTO IDType)
    {
        _logger.LogInformation("Creating IDType: {IDTypeName}", IDType.IDTypeName);

        var isDuplicate = await _idTypeRepository.IsDuplicateAsync(IDType, 0);

        if (isDuplicate)
        {
            _logger.LogWarning("IDType creation failed - duplicate found: {IDTypeName}",
                IDType.IDTypeName);
            throw new BusinessRuleException(
                "IDType already registered in the system."
            );
        }

        var outID = await _idTypeRepository.DataProcessAsync(1, IDType, 0, userId: 1);

        if (outID <= 0)
        {
            _logger.LogError("IDType creation failed - invalid ID returned: {OutID}", outID);
            throw new BusinessRuleException("Failed to register IDType. Database operation did not succeed.");
        }

        _logger.LogInformation("IDType created successfully: {IDTypeId}", outID);
        return outID;
    }

    public async Task UpdateIDTypeAsync(IDTypeReqDTO IDType, int idTypeId)
    {
        _logger.LogInformation("Updating IDType {IDTypeId}", idTypeId);

        if (idTypeId <= 0)
        {
            throw new ArgumentException("Valid IDType ID is required for update.", nameof(idTypeId));
        }

        var exists = await _idTypeRepository.ExistsAsync(idTypeId);
        if (!exists)
        {
            _logger.LogWarning("IDType update failed - IDType not found: {IDTypeId}", idTypeId);
            throw new NotFoundException("IDType", idTypeId);
        }

        var isDuplicate = await _idTypeRepository.IsDuplicateAsync(IDType, idTypeId);
        if (isDuplicate)
        {
            _logger.LogWarning("IDType update failed - duplicate found: {IDTypeId}", idTypeId);
            throw new BusinessRuleException("A IDType with this IDTypeName is already registered in the system.");
        }

        var outID = await _idTypeRepository.DataProcessAsync(2, IDType, idTypeId, userId: 1);
        if (outID <= 0)
        {
            _logger.LogError("IDType update failed - invalid ID returned: {IDTypeId}", idTypeId);
            throw new BusinessRuleException("Failed to update IDType. Database operation did not succeed.");
        }

        _logger.LogInformation("IDType updated successfully: {IDTypeId}", idTypeId);
    }

    public async Task<IDTypeReqDTO> GetIDTypeByIdAsync(int idTypeId)
    {
        var result = await _idTypeRepository.GetByIdAsync(idTypeId);
        return result ?? throw new NotFoundException("IDType", idTypeId);
    }

    public async Task<PagedResult<IDTypeRespDTO>> GetIDTypeByPageAsync(PageQuery query, CancellationToken ct)
    {
        return await _idTypeRepository.GetByPageAsync(query, ct);
    }

    public async Task DeleteIDTypeAsync(int idTypeId)
    {
        _logger.LogInformation("Deleting IDType {IDTypeId} by user {UserId}", idTypeId, 1);

        if (idTypeId <= 0)
        {
            throw new ArgumentException("Valid IDType ID is required for deletion.", nameof(idTypeId));
        }

        var exists = await _idTypeRepository.ExistsAsync(idTypeId);
        if (!exists)
        {
            _logger.LogWarning("IDType deletion failed - IDType not found: {IDTypeId}", idTypeId);
            throw new NotFoundException("IDType", idTypeId);
        }

        var outID = await _idTypeRepository.DataProcessAsync(3, new IDTypeReqDTO { }, idTypeId, userId: 1);
        if (outID <= 0)
        {
            _logger.LogError("IDType deletion failed - invalid ID returned: {IDTypeId}", idTypeId);
            throw new BusinessRuleException("Failed to delete IDType. Database operation did not succeed.");
        }

        _logger.LogInformation("IDType deleted successfully: {IDTypeId}", idTypeId);
    }
}
