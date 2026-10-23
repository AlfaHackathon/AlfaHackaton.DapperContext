using System.Data;
using AlfaHackaton.DapperContext.Common.Interfaces.Dapper;
using Dapper;
using Npgsql;

namespace AlfaHackaton.DapperContext.Dapper;

public class Transaction : ITransaction
{
    private readonly IDbConnection _connection;
    private readonly IDbTransaction _transaction;
    
    public Transaction(string connectionString)
    {
        _connection = new NpgsqlConnection(connectionString);

        if (_connection.State != ConnectionState.Open)
        {
            _connection.Open();
        }

        _transaction = _connection.BeginTransaction();
    }

    public void Dispose()
    {
        _transaction.Dispose();
        _connection.Close();
        _connection.Dispose();
    }

    public void Commit()
    {
        _transaction.Commit();
    }

    public void Rollback()
    {
        _transaction.Rollback();
    }

    public Task<T?> FirstOrDefault<T>(IQueryObject queryObject)
    {
        return _connection.QueryFirstOrDefaultAsync<T>(queryObject.Sql, queryObject.Parameters, _transaction);
    }

    public async Task<List<T>> ListOrEmpty<T>(IQueryObject queryObject)
    {
        return (await _connection.QueryAsync<T>(queryObject.Sql, queryObject.Parameters, _transaction).ConfigureAwait(false)).AsList();
    }

    public Task<int> CommandWithCount(IQueryObject queryObject)
    {
        return _connection.ExecuteAsync(queryObject.Sql, queryObject.Parameters, _transaction);
    }

    public async Task Command(IQueryObject queryObject)
    {
        await _connection.ExecuteAsync(queryObject.Sql, queryObject.Parameters, _transaction);
    }

    public async Task<T> CommandWithResponse<T>(IQueryObject queryObject)
    {
        return await _connection.QueryFirstAsync<T>(queryObject.Sql, queryObject.Parameters, _transaction);
    }
}
