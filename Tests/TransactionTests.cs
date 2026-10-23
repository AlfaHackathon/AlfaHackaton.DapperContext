using AlfaHackaton.DapperContext.Common.Interfaces.Dapper.Settings;
using AlfaHackaton.DapperContext.Dapper;
using Dapper;
using Npgsql;
using Xunit;

namespace AlfaHackaton.DapperContext.Tests;

public sealed class TransactionTests : IAsyncLifetime
{
    private readonly string database = "dapper_transaction_" + Guid.NewGuid().ToString("N");
    private readonly string admin = Environment.GetEnvironmentVariable("DAPPER_TEST_POSTGRES")
        ?? throw new InvalidOperationException("Set DAPPER_TEST_POSTGRES to an isolated PostgreSQL instance.");
    private Dapper.DapperContext context = null!;

    public async Task InitializeAsync()
    {
        await using var connection = new NpgsqlConnection(admin);
        await connection.ExecuteAsync($"CREATE DATABASE {database}");
        var settings = new Settings
        {
            ConnectionString = new NpgsqlConnectionStringBuilder(admin) { Database = database, Pooling = false }.ConnectionString
        };
        context = new Dapper.DapperContext(settings);
        await context.Command(new QueryObject("CREATE TABLE items (id integer PRIMARY KEY, label text NOT NULL)"));
    }

    public async Task DisposeAsync()
    {
        await using var connection = new NpgsqlConnection(admin);
        await connection.ExecuteAsync($"DROP DATABASE IF EXISTS {database} WITH (FORCE)");
    }

    [Fact]
    public async Task ReadsOwnWritesWithoutExposingThemToOtherConnections()
    {
        using var transaction = context.BeginTransaction();
        await transaction.Command(new QueryObject("INSERT INTO items VALUES (@id, @label)", new { id = 7, label = "pending" }));

        var row = await transaction.FirstOrDefault<Item>(new QueryObject("SELECT id, label FROM items WHERE id=@id", new { id = 7 }));
        Assert.NotNull(row);
        Assert.Equal(new Item(7, "pending"), row);
        Assert.Equal([row], await transaction.ListOrEmpty<Item>(new QueryObject("SELECT id, label FROM items ORDER BY id")));
        Assert.Equal(0, await context.FirstOrDefault<int>(new QueryObject("SELECT COUNT(*) FROM items")));

        transaction.Commit();
        Assert.Equal(1, await context.FirstOrDefault<int>(new QueryObject("SELECT COUNT(*) FROM items")));
    }

    [Fact]
    public async Task EmptyReadsAndAffectedCountsPreserveRollbackOnDispose()
    {
        using (var transaction = context.BeginTransaction())
        {
            Assert.Null(await transaction.FirstOrDefault<Item>(new QueryObject("SELECT id, label FROM items")));
            Assert.Empty(await transaction.ListOrEmpty<Item>(new QueryObject("SELECT id, label FROM items")));
            Assert.Equal(2, await transaction.CommandWithCount(new QueryObject("INSERT INTO items VALUES (1,'a'),(2,'b')")));
            Assert.Equal(1, await transaction.CommandWithCount(new QueryObject("UPDATE items SET label=@label WHERE id=@id", new { id = 1, label = "changed" })));
            Assert.Equal(0, await transaction.CommandWithCount(new QueryObject("DELETE FROM items WHERE id=@id", new { id = 999 })));
            Assert.Equal(2, await transaction.CommandWithResponse<long>(new QueryObject("SELECT COUNT(*) FROM items")));
        }

        Assert.Equal(0, await context.FirstOrDefault<int>(new QueryObject("SELECT COUNT(*) FROM items")));
    }

    [Fact]
    public async Task NullableScalarAndExplicitRollbackRemainSupported()
    {
        using var transaction = context.BeginTransaction();
        Assert.Null(await transaction.FirstOrDefault<long?>(new QueryObject("SELECT id FROM items")));
        await transaction.CommandWithCount(new QueryObject("INSERT INTO items VALUES (1,'rollback')"));
        transaction.Rollback();
        Assert.Empty(await context.ListOrEmpty<Item>(new QueryObject("SELECT id, label FROM items")));
    }

    public sealed record Item(int Id, string Label);

    private sealed class Settings : IDapperSettings
    {
        public required string ConnectionString { get; set; }
    }
}
