using System.Data;
using System.Data.Common;
using Dapper;
using Microsoft.Extensions.Logging;

namespace OneMonthFlow.Databases
{
    public class MySqlService : ISqlService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<MySqlService> _logger;

        public MySqlService(IDbConnectionFactory connectionFactory, ILogger<MySqlService> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task<int> ExecuteAsync(string sql, object parameters = null)
        {
            try
            {
                _logger.LogInformation("Executing SQL: {Sql} | Parameters: {@Parameters}", sql, parameters);

                using var connection = _connectionFactory.CreateConnection();
                await ((DbConnection)connection).OpenAsync();

                var result = await connection.ExecuteAsync(sql, parameters);

                _logger.LogInformation("Execution successful. Rows affected: {Result}", result);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing SQL: {Sql} | Parameters: {@Parameters}", sql, parameters);
                throw;
            }
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object parameters = null)
        {
            try
            {
                _logger.LogInformation("Querying SQL: {Sql} | Parameters: {@Parameters}", sql, parameters);

                using var connection = _connectionFactory.CreateConnection();
                await ((DbConnection)connection).OpenAsync();

                var result = await connection.QueryAsync<T>(sql, parameters);

                _logger.LogInformation("Query successful. Rows returned: {Count}", result.Count());

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying SQL: {Sql} | Parameters: {@Parameters}", sql, parameters);
                throw;
            }
        }

        public async Task<T> QuerySingleAsync<T>(string sql, object parameters = null)
        {
            try
            {
                _logger.LogInformation("Querying single result SQL: {Sql} | Parameters: {@Parameters}", sql, parameters);

                using var connection = _connectionFactory.CreateConnection();
                await ((DbConnection)connection).OpenAsync();

                var result = await connection.QuerySingleAsync<T>(sql, parameters);

                _logger.LogInformation("Single result query successful.");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying single result SQL: {Sql} | Parameters: {@Parameters}", sql, parameters);
                throw;
            }
        }

        public async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object parameters = null)
        {
            try
            {
                _logger.LogInformation("Querying first or default SQL: {Sql} | Parameters: {@Parameters}", sql, parameters);

                using var connection = _connectionFactory.CreateConnection();
                await ((DbConnection)connection).OpenAsync();

                var result = await connection.QueryFirstOrDefaultAsync<T>(sql, parameters);

                _logger.LogInformation("First or default query successful. Result: {@Result}", result);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying first or default SQL: {Sql} | Parameters: {@Parameters}", sql, parameters);
                throw;
            }
        }
    }
}
