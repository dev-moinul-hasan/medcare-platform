using Dapper;
using MedCare.Application.Abstractions.Others;
using MedCare.Application.Abstractions.Repositories.Core;
using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;
using Microsoft.Extensions.Logging;
using System.Data;

namespace MedCare.Infrastructure.Repositories.NewFolder.Core;

public class CityRepository(ISqlConnectionFactory connectionFactory, ILogger<CityRepository> logger) : ICityRepository
{
    public async Task<int> DataProcessAsync(int action, CityReqDTO command, int cityId, int userId)
    {
        var actionName = action switch
        {
            1 => "Insert City",
            2 => "Update City",
            3 => "Delete City",
            _ => "Unknown Action"
        };

        using var connection = connectionFactory.CreateConnection();

        var dp = new DynamicParameters();
        dp.Add("@Action", action, DbType.Int32);
        dp.Add("@CityId", cityId, DbType.Int32);
        dp.Add("@CityName", command.CityName, DbType.String, ParameterDirection.Input, 100);
        dp.Add("@StateId", command.StateId, DbType.Int32);
        dp.Add("@IsActive", command.IsActive ? 1:0, DbType.Boolean, ParameterDirection.Input);

        dp.Add("@error", dbType: DbType.Int32, direction: ParameterDirection.Output);
        dp.Add("@OutID", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync("core.[usp_City]", dp, commandType: CommandType.StoredProcedure);

        var error = dp.Get<int>("@error");
        var outID = dp.Get<int>("@OutID");

        if (error != 0)
        {
            logger.LogError(
                "Stored Procedure Error: {Action} failed with error code {ErrorCode}. " +
                "CityID: {CityId}, OutID: {OutID}, UserID: {UserId}",
                actionName, error, cityId, outID, userId);
        }

        return outID;
    }

    public async Task<CityReqDTO?> GetByIdAsync(int cityId)
    {
        const string sql = @"SELECT CityId, CityName, IsActive FROM core.City WHERE CityId=@cityId";

        using var connection = connectionFactory.CreateConnection();

        try
        {
            var result = await connection.QueryFirstOrDefaultAsync<CityReqDTO>(sql, new { CityId = cityId });
            logger.LogInformation("Retrieved city {CityId}", cityId);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving city {CityId}", cityId);
            throw;
        }
    }

    public async Task<PagedResult<CityRespDTO>> GetByPageAsync(PageQuery query, CancellationToken ct)
    {
        var sortColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["city"] = "CityName"
        };

        var sortColumn = sortColumns.GetValueOrDefault(query.SortBy ?? "", "CityName");
        var direction = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";

        var sql = $@"
                SELECT CityId, CityName, IsActive, COUNT(*) OVER() AS TotalCount FROM core.City WHERE @Search IS NULL OR CityName LIKE @Search 
                ORDER BY {sortColumn} {direction}, CityId OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY";

        var args = new
        {
            Search = string.IsNullOrWhiteSpace(query.Search) ? null : $"{query.Search.Trim()}%",
            Skip = (query.Page - 1) * query.PageSize,
            query.PageSize
        };

        using var connection = connectionFactory.CreateConnection();

        try
        {
            var rows = (await connection.QueryAsync<CityRespDTO, int, (CityRespDTO Item, int Total)>(
                new CommandDefinition(sql, args, cancellationToken: ct),
                (item, total) => (item, total),
                splitOn: "TotalCount")).AsList();

            var total = rows.Count > 0 ? rows[0].Total : 0;
            logger.LogInformation("Retrieved {Count} city (page {Page}, total {Total})", rows.Count, query.Page, total);

            return new PagedResult<CityRespDTO>(rows.ConvertAll(r => r.Item), total, query.Page, query.PageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving city for page {Page}", query.Page);
            throw;
        }
    }

    public async Task<bool> IsDuplicateAsync(CityReqDTO city, int cityId)
    {
        const string sql = @"SELECT CASE WHEN EXISTS (SELECT 1 FROM core.City WHERE UPPER(CityName) = UPPER(@CityName) AND CityId !=@cityId) THEN 1 ELSE 0 END";

        using var connection = connectionFactory.CreateConnection();

        try
        {
            var isDuplicate = await connection.ExecuteScalarAsync<bool>(sql, new { city.CityName, cityId });
            if (isDuplicate)
            {
                logger.LogWarning("Duplicate City detected: CityName={CityName}",
                    city.CityName);
            }
            return isDuplicate;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking duplicate city");
            throw;
        }
    }

    public async Task<bool> ExistsAsync(int cityId)
    {
        const string sql = "SELECT CASE WHEN EXISTS (SELECT 1 FROM core.City WHERE CityId = @CityId) THEN 1 ELSE 0 END";

        using var connection = connectionFactory.CreateConnection();

        try
        {
            var exists = await connection.ExecuteScalarAsync<bool>(sql, new { CityId = cityId });
            logger.LogDebug("Patient {CityId} exists: {Exists}", cityId, exists);
            return exists;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking city existence {CityId}", cityId);
            throw;
        }
    }
}
