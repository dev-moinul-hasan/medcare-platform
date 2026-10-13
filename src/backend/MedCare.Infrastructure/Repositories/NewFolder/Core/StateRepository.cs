using Dapper;
using MedCare.Application.Abstractions.Others;
using MedCare.Application.Abstractions.Repositories.Core;
using MedCare.Application.DTOs.Common;
using MedCare.Application.DTOs.Requests.Core;
using MedCare.Application.DTOs.Responses.Core;
using Microsoft.Extensions.Logging;
using System.Data;

namespace MedCare.Infrastructure.Repositories.NewFolder.Core;

public class StateRepository(ISqlConnectionFactory connectionFactory, ILogger<StateRepository> logger) : IStateRepository
{
    public async Task<int> DataProcessAsync(int action, StateReqDTO command, int stateId, int userId)
    {
        var actionName = action switch
        {
            1 => "Insert State",
            2 => "Update State",
            3 => "Delete State",
            _ => "Unknown Action"
        };

        using var connection = connectionFactory.CreateConnection();

        var dp = new DynamicParameters();
        dp.Add("@Action", action, DbType.Int32);
        dp.Add("@StateId", stateId, DbType.Int32);
        dp.Add("@StateName", command.StateName, DbType.String, ParameterDirection.Input, 100);
        dp.Add("@CountryId", command.CountryId, DbType.Int32, ParameterDirection.Input);
        dp.Add("@IsActive", command.IsActive ? 1:0, DbType.Boolean, ParameterDirection.Input);

        dp.Add("@error", dbType: DbType.Int32, direction: ParameterDirection.Output);
        dp.Add("@OutID", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync("core.[usp_State]", dp, commandType: CommandType.StoredProcedure);

        var error = dp.Get<int>("@error");
        var outID = dp.Get<int>("@OutID");

        if (error != 0)
        {
            logger.LogError(
                "Stored Procedure Error: {Action} failed with error code {ErrorCode}. " +
                "StateID: {StateId}, OutID: {OutID}, UserID: {UserId}",
                actionName, error, stateId, outID, userId);
        }

        return outID;
    }

    public async Task<StateReqDTO?> GetByIdAsync(int stateId)
    {
        const string sql = @"SELECT StateId, StateName, CountryId, IsActive FROM core.State WHERE StateId=@stateId";

        using var connection = connectionFactory.CreateConnection();

        try
        {
            var result = await connection.QueryFirstOrDefaultAsync<StateReqDTO>(sql, new { StateId = stateId });
            logger.LogInformation("Retrieved state {StateId}", stateId);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving state {StateId}", stateId);
            throw;
        }
    }

    public async Task<PagedResult<StateRespDTO>> GetByPageAsync(PageQuery query, CancellationToken ct)
    {
        var sortColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["stateName"] = "s.StateName",
            ["countryName"] = "c.ContryName"
        };

        var sortColumn = sortColumns.GetValueOrDefault(query.SortBy ?? "", "StateName");
        var direction = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";

        var sql = $@"SELECT s.StateId, s.StateName, s.CountryId,c.CountryName, s.IsActive, COUNT(*) OVER() AS TotalCount FROM core.State s INNER JOIN core.Country c  ON s.CountryId =  c.CountryId WHERE @Search IS NULL OR s.StateName LIKE @Search OR c.CountryName LIKE @Search
                ORDER BY {sortColumn} {direction}, s.StateId OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY";

        var args = new
        {
            Search = string.IsNullOrWhiteSpace(query.Search) ? null : $"{query.Search.Trim()}%",
            Skip = (query.Page - 1) * query.PageSize,
            query.PageSize
        };

        using var connection = connectionFactory.CreateConnection();

        try
        {
            var rows = (await connection.QueryAsync<StateRespDTO, int, (StateRespDTO Item, int Total)>(
                new CommandDefinition(sql, args, cancellationToken: ct),
                (item, total) => (item, total),
                splitOn: "TotalCount")).AsList();

            var total = rows.Count > 0 ? rows[0].Total : 0;
            logger.LogInformation("Retrieved {Count} state (page {Page}, total {Total})", rows.Count, query.Page, total);

            return new PagedResult<StateRespDTO>(rows.ConvertAll(r => r.Item), total, query.Page, query.PageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving state for page {Page}", query.Page);
            throw;
        }
    }

    public async Task<bool> IsDuplicateAsync(StateReqDTO state, int stateId)
    {
        const string sql = @"SELECT CASE WHEN EXISTS (SELECT 1 FROM core.State WHERE UPPER(StateName) = UPPER(@StateName) And StateId != @stateId) THEN 1 ELSE 0 END";

        using var connection = connectionFactory.CreateConnection();

        try
        {
            var isDuplicate = await connection.ExecuteScalarAsync<bool>(sql, new { state.StateName, stateId });
            if (isDuplicate)
            {
                logger.LogWarning("Duplicate State detected: StateName={StateName}",
                    state.StateName);
            }
            return isDuplicate;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking duplicate state");
            throw;
        }
    }

    public async Task<bool> ExistsAsync(int stateId)
    {
        const string sql = "SELECT CASE WHEN EXISTS (SELECT 1 FROM core.State WHERE StateId = @StateId) THEN 1 ELSE 0 END";

        using var connection = connectionFactory.CreateConnection();

        try
        {
            var exists = await connection.ExecuteScalarAsync<bool>(sql, new { StateId = stateId });
            logger.LogDebug("Patient {StateId} exists: {Exists}", stateId, exists);
            return exists;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking state existence {StateId}", stateId);
            throw;
        }
    }
}
