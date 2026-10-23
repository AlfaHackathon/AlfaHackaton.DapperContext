namespace AlfaHackaton.DapperContext.Common.Interfaces.Dapper;

public interface ITransaction : IDisposable
{
    Task<T?> FirstOrDefault<T>(IQueryObject queryObject);
    Task<List<T>> ListOrEmpty<T>(IQueryObject queryObject);
    Task<int> CommandWithCount(IQueryObject queryObject);
    Task<T> CommandWithResponse<T>(IQueryObject queryObject);
    Task Command(IQueryObject queryObject);
    public void Commit();
    public void Rollback();
}
