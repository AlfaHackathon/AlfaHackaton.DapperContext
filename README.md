# AlfaHackaton.DapperContext

## Transaction queries (1.2.0)

`ITransaction.FirstOrDefault<T>`, `ListOrEmpty<T>` and `CommandWithCount` use
the same connection and transaction as `Command` and `CommandWithResponse`.
Use them for read-your-writes and atomic operations. The similarly named
methods on `IDapperContext` open separate connections.

`CommandWithCount` returns Dapper's affected row count. `FirstOrDefault<T>`
returns default when no row exists; use a nullable value type when absence
must be distinguished from zero. Existing transaction methods retain their behavior.

Adding members to `ITransaction` requires third-party implementations to implement
the three new methods when upgrading. The packaged `Transaction` implements all of them.

Integration tests require a disposable PostgreSQL database creation grant:

```powershell
$env:DAPPER_TEST_POSTGRES = 'Host=127.0.0.1;Port=55441;Database=postgres;Username=postgres;Pooling=false'
dotnet test Tests/AlfaHackaton.DapperContext.Tests.csproj
```

The tests create and drop uniquely named databases. They verify uncommitted visibility,
typed and empty reads, affected row counts, commit and rollback.
