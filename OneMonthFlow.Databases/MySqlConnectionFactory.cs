using System.Data;
using MySql.Data.MySqlClient;

namespace OneMonthFlow.Databases
{
    public class MySqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public MySqlConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }
    }
}
