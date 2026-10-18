namespace AlfaHackaton.DapperContext.Common.Interfaces.Dapper;

public interface ITransaction : IDisposable
{
    Task<T> CommandWithResponse<T>(IQueryObject queryObject);
    Task Command(IQueryObject queryObject);
    public void Commit();
    public void Rollback();
}