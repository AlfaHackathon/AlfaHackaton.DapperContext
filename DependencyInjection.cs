using System.Reflection;
using AlfaHackaton.DapperContext.Common.Interfaces.Dapper;
using AlfaHackaton.DapperContext.Common.Interfaces.Dapper.Settings;
using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlfaHackaton.DapperContext;

public static class DependencyInjection
{
    public static IServiceCollection AddDapper(this IServiceCollection services, IDapperSettings dapperSettings)
    {
        services.AddSingleton<IDapperSettings>(dapperSettings);
        services.AddSingleton<IDapperContext, Dapper.DapperContext>();
        return services;
    }
    
    public static IServiceCollection AddMigrations(this IServiceCollection services, IDapperSettings dapperSettings, Assembly migrationsAssembly)
    {
        services
            .AddLogging(c => c.AddFluentMigratorConsole())
            .AddFluentMigratorCore()
            .ConfigureRunner(c => c
                .AddPostgres()
                .WithGlobalConnectionString(dapperSettings.ConnectionString)
                .ScanIn(migrationsAssembly));
        
        return services;
    }

    public static IServiceProvider UseMigrations(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var runner = scope.ServiceProvider.GetService<IMigrationRunner>();
        runner.MigrateUp();
        
        return serviceProvider;
    }
}