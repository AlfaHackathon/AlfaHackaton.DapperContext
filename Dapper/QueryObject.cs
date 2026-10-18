using AlfaHackaton.DapperContext.Common.Interfaces.Dapper;

namespace AlfaHackaton.DapperContext.Dapper;

public class QueryObject : IQueryObject
{
    public QueryObject(string sql, object? parameters = null)
    {
        if (string.IsNullOrEmpty(sql))
        {
            throw new ArgumentException("Sql не может быть пустым");
        }
        
        Sql = sql;
        Parameters = parameters;
    }
    
    public string Sql { get; set; }
    public object? Parameters { get; set; }
}