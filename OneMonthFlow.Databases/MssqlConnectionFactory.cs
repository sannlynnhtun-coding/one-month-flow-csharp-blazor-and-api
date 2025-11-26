using System.Data;
using Microsoft.Data.SqlClient;

namespace OneMonthFlow.Databases
{
    public class MssqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public MssqlConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}

