namespace AlfaHackaton.DapperContext.Common.Interfaces.Dapper;

public interface IDapperContext
{
    public ITransaction BeginTransaction();
    public Task Command(IQueryObject queryObject, ITransaction? transaction = null);
    public Task<T> CommandWithResponse<T>(IQueryObject queryObject, ITransaction? transaction = null);
    public Task<T?> FirstOrDefault<T>(IQueryObject queryObject);
    public Task<List<T>> ListOrEmpty<T>(IQueryObject queryObject);
}