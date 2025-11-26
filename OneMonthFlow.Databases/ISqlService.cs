
namespace OneMonthFlow.Databases
{
    public interface ISqlService
    {
        Task<int> ExecuteAsync(string sql, object parameters = null);
        Task<IEnumerable<T>> QueryAsync<T>(string sql, object parameters = null);
        Task<T> QueryFirstOrDefaultAsync<T>(string sql, object parameters = null);
        Task<T> QuerySingleAsync<T>(string sql, object parameters = null);
    }
}