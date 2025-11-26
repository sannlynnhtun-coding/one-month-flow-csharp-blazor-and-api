using System.Data;

namespace OneMonthFlow.Databases
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
