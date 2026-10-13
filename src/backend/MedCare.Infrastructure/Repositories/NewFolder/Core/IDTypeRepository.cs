using Dapper;
using MedCare.Application.Abstractions.Others;
using MedCare.Application.Abstractions.Repositories.Core;
using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;
using Microsoft.Extensions.Logging;
using System.Data;

namespace MedCare.Infrastructure.Repositories.NewFolder.Core;

public class IDTypeRepository(ISqlConnectionFactory connectionFactory, ILogger<IDTypeRepository> logger) : IIDTypeRepository
{
    public async Task<int> DataProcessAsync(int action, IDTypeReqDTO command, int idType, int userId)
    {
        var actionName = action switch
        {
            1 => "Insert IDType",
            2 => "Update IDType",
            3 => "Delete IDType",
            _ => "Unknown Action"
        };

        using var connection = connectionFactory.CreateConnection();

        var dp = new DynamicParameters();
        dp.Add("@Action", action, DbType.Int32);
        dp.Add("@IDTypeId", idType, DbType.Int32);
        dp.Add("@IDTypeName", command.IDTypeName, DbType.String, ParameterDirection.Input, 50);
        dp.Add("@IsActive", command.IsActive ? 1:0, DbType.Boolean, ParameterDirection.Input);

        dp.Add("@error", dbType: DbType.Int32, direction: ParameterDirection.Output);
        dp.Add("@OutID", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync("core.[usp_IDType]", dp, commandType: CommandType.StoredProcedure);

        var error = dp.Get<int>("@error");
        var outID = dp.Get<int>("@OutID");

        if (error != 0)
        {
            logger.LogError(
                "Stored Procedure Error: {Action} failed with error code {ErrorCode}. " +
                "IDTypeID: {IDTypeId}, OutID: {OutID}, UserID: {UserId}",
                actionName, error, idType, outID, userId);
        }

        return outID;
    }

    public async Task<IDTypeReqDTO?> GetByIdAsync(int idTypeId)
    {
        const string sql = @"SELECT IDTypeId, IDTypeName, IsActive FROM core.IDType WHERE IDTypeId=@idTypeId";

        using var connection = connectionFactory.CreateConnection();

        try
        {
            var result = await connection.QueryFirstOrDefaultAsync<IDTypeReqDTO>(sql, new { IDTypeId = idTypeId });
            logger.LogInformation("Retrieved idType {IDTypeId}", idTypeId);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving idType {IDTypeId}", idTypeId);
            throw;
        }
    }

    public async Task<PagedResult<IDTypeRespDTO>> GetByPageAsync(PageQuery query, CancellationToken ct)
    {
        var sortColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["idType"] = "IDTypeName"
        };

        var sortColumn = sortColumns.GetValueOrDefault(query.SortBy ?? "", "IDTypeName");
        var direction = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";

        var sql = $@"
                SELECT IDTypeId, IDTypeName, IsActive, COUNT(*) OVER() AS TotalCount FROM core.IDType WHERE @Search IS NULL OR IDTypeName LIKE @Search 
                ORDER BY {sortColumn} {direction}, IDTypeId OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY";

        var args = new
        {
            Search = string.IsNullOrWhiteSpace(query.Search) ? null : $"{query.Search.Trim()}%",
            Skip = (query.Page - 1) * query.PageSize,
            query.PageSize
        };

        using var connection = connectionFactory.CreateConnection();

        try
        {
            var rows = (await connection.QueryAsync<IDTypeRespDTO, int, (IDTypeRespDTO Item, int Total)>(
                new CommandDefinition(sql, args, cancellationToken: ct),
                (item, total) => (item, total),
                splitOn: "TotalCount")).AsList();

            var total = rows.Count > 0 ? rows[0].Total : 0;
            logger.LogInformation("Retrieved {Count} idType (page {Page}, total {Total})", rows.Count, query.Page, total);

            return new PagedResult<IDTypeRespDTO>(rows.ConvertAll(r => r.Item), total, query.Page, query.PageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving idType for page {Page}", query.Page);
            throw;
        }
    }

    public async Task<bool> IsDuplicateAsync(IDTypeReqDTO idType, int idTypeId)
    {
        const string sql = @"SELECT CASE WHEN EXISTS (SELECT 1 FROM core.IDType WHERE UPPER(IDTypeName) = UPPER(@IDTypeName) AND IDTypeId != @idTypeId) THEN 1 ELSE 0 END";

        using var connection = connectionFactory.CreateConnection();

        try
        {
            var isDuplicate = await connection.ExecuteScalarAsync<bool>(sql, new { idType.IDTypeName, idTypeId });
            if (isDuplicate)
            {
                logger.LogWarning("Duplicate IDType detected: IDTypeName={IDTypeName}",
                    idType.IDTypeName);
            }
            return isDuplicate;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking duplicate idType");
            throw;
        }
    }

    public async Task<bool> ExistsAsync(int idTypeId)
    {
        const string sql = "SELECT CASE WHEN EXISTS (SELECT 1 FROM core.IDType WHERE IDTypeId = @IDTypeId) THEN 1 ELSE 0 END";

        using var connection = connectionFactory.CreateConnection();

        try
        {
            var exists = await connection.ExecuteScalarAsync<bool>(sql, new { IDTypeId = idTypeId });
            logger.LogDebug("Patient {IDTypeId} exists: {Exists}", idTypeId, exists);
            return exists;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking idType existence {IDTypeId}", idTypeId);
            throw;
        }
    }
}
