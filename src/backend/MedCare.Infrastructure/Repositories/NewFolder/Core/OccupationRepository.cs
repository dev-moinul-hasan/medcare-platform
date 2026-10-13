using Dapper;
using MedCare.Application.Abstractions.Others;
using MedCare.Application.Abstractions.Repositories.Core;
using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;
using Microsoft.Extensions.Logging;
using System.Data;

namespace MedCare.Infrastructure.Repositories.NewFolder.Core;

public class OccupationRepository(ISqlConnectionFactory connectionFactory, ILogger<OccupationRepository> logger) : IOccupationRepository
{
    public async Task<int> DataProcessAsync(int action, OccupationReqDTO command, int occupation, int userId)
    {
        var actionName = action switch
        {
            1 => "Insert Occupation",
            2 => "Update Occupation",
            3 => "Delete Occupation",
            _ => "Unknown Action"
        };

        using var connection = connectionFactory.CreateConnection();

        var dp = new DynamicParameters();
        dp.Add("@Action", action, DbType.Int32);
        dp.Add("@OccupationId", occupation, DbType.Int32);
        dp.Add("@OccupationName", command.OccupationName, DbType.String, ParameterDirection.Input, 100);
        dp.Add("@IsActive", command.IsActive ? 1:0, DbType.Boolean, ParameterDirection.Input);

        dp.Add("@error", dbType: DbType.Int32, direction: ParameterDirection.Output);
        dp.Add("@OutID", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync("core.[usp_Occupation]", dp, commandType: CommandType.StoredProcedure);

        var error = dp.Get<int>("@error");
        var outID = dp.Get<int>("@OutID");

        if (error != 0)
        {
            logger.LogError(
                "Stored Procedure Error: {Action} failed with error code {ErrorCode}. " +
                "OccupationID: {OccupationId}, OutID: {OutID}, UserID: {UserId}",
                actionName, error, occupation, outID, userId);
        }

        return outID;
    }

    public async Task<OccupationReqDTO?> GetByIdAsync(int occupationId)
    {
        const string sql = @"SELECT OccupationId, OccupationName, IsActive FROM core.Occupation WHERE OccupationId=@occupationId";

        using var connection = connectionFactory.CreateConnection();

        try
        {
            var result = await connection.QueryFirstOrDefaultAsync<OccupationReqDTO>(sql, new { OccupationId = occupationId });
            logger.LogInformation("Retrieved occupation {OccupationId}", occupationId);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving occupation {OccupationId}", occupationId);
            throw;
        }
    }

    public async Task<PagedResult<OccupationRespDTO>> GetByPageAsync(PageQuery query, CancellationToken ct)
    {
        var sortColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["occupation"] = "OccupationName"
        };

        var sortColumn = sortColumns.GetValueOrDefault(query.SortBy ?? "", "OccupationName");
        var direction = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";

        var sql = $@"
                SELECT OccupationId, OccupationName, IsActive, COUNT(*) OVER() AS TotalCount FROM core.Occupation WHERE @Search IS NULL OR OccupationName LIKE @Search 
                ORDER BY {sortColumn} {direction}, OccupationId OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY";

        var args = new
        {
            Search = string.IsNullOrWhiteSpace(query.Search) ? null : $"{query.Search.Trim()}%",
            Skip = (query.Page - 1) * query.PageSize,
            query.PageSize
        };

        using var connection = connectionFactory.CreateConnection();

        try
        {
            var rows = (await connection.QueryAsync<OccupationRespDTO, int, (OccupationRespDTO Item, int Total)>(
                new CommandDefinition(sql, args, cancellationToken: ct),
                (item, total) => (item, total),
                splitOn: "TotalCount")).AsList();

            var total = rows.Count > 0 ? rows[0].Total : 0;
            logger.LogInformation("Retrieved {Count} occupation (page {Page}, total {Total})", rows.Count, query.Page, total);

            return new PagedResult<OccupationRespDTO>(rows.ConvertAll(r => r.Item), total, query.Page, query.PageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving occupation for page {Page}", query.Page);
            throw;
        }
    }

    public async Task<bool> IsDuplicateAsync(OccupationReqDTO occupation, int occupationId)
    {
        const string sql = @"SELECT CASE WHEN EXISTS (SELECT 1 FROM core.Occupation WHERE UPPER(OccupationName) = UPPER(@OccupationName) AND OccupationId !=@occupationId) THEN 1 ELSE 0 END";

        using var connection = connectionFactory.CreateConnection();

        try
        {
            var isDuplicate = await connection.ExecuteScalarAsync<bool>(sql, new { occupation.OccupationName, occupationId });
            if (isDuplicate)
            {
                logger.LogWarning("Duplicate Occupation detected: OccupationName={OccupationName}",
                    occupation.OccupationName);
            }
            return isDuplicate;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking duplicate occupation");
            throw;
        }
    }

    public async Task<bool> ExistsAsync(int occupationId)
    {
        const string sql = "SELECT CASE WHEN EXISTS (SELECT 1 FROM core.Occupation WHERE OccupationId = @OccupationId) THEN 1 ELSE 0 END";

        using var connection = connectionFactory.CreateConnection();

        try
        {
            var exists = await connection.ExecuteScalarAsync<bool>(sql, new { OccupationId = occupationId });
            logger.LogDebug("Patient {OccupationId} exists: {Exists}", occupationId, exists);
            return exists;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking occupation existence {OccupationId}", occupationId);
            throw;
        }
    }
}
