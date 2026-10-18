namespace AlfaHackaton.DapperContext.Common.Interfaces.Dapper;

public interface IQueryObject
{
    public string Sql { get; set; }
    public object? Parameters { get; set; }
}