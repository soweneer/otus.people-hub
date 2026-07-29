using Npgsql;
using PeopleHub.Chats.Db;
using PeopleHub.Chats.Repositories;
using PeopleHub.Chats.Services;
using PeopleHub.Chats.Tarantool;

namespace PeopleHub.Chats;

public static class Bootstrapper
{
    private const int DefaultTarantoolPoolSize = 32;
    private const int DefaultTarantoolReadBufferSize = 262144;

    public static IServiceCollection AddChats(this IServiceCollection services, IConfiguration configuration)
    {
        return configuration.GetValue<bool>("FeatureFlags:UseTarantoolStorage")
            ? AddTarantoolDialogs(services, configuration)
            : AddPostgresDialogs(services, configuration);
    }

    private static IServiceCollection AddPostgresDialogs(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSql");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new MissingMemberException("Connection string is absent");
        }

        services.AddSingleton(new NpgsqlDataSourceBuilder(connectionString).Build());
        services.AddScoped<DbClient>();
        services.AddScoped<IDialogRepository, DialogRepository>();
        services.AddScoped<IDialogService, DialogService>();
        services.AddScoped<IDbMigrator, DbMigrator>();

        return services;
    }

    private static IServiceCollection AddTarantoolDialogs(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Tarantool");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new MissingMemberException("Tarantool connection string is absent");
        }

        var poolSize = configuration.GetValue<int?>("Dialogs:Tarantool:PoolSize") ?? DefaultTarantoolPoolSize;
        var readBufferSize = configuration.GetValue<int?>("Dialogs:Tarantool:ReadBufferSize") ?? DefaultTarantoolReadBufferSize;

        services.AddSingleton(new TarantoolConnectionPool(connectionString, poolSize, readBufferSize));
        services.AddSingleton<IDialogService, TarantoolDialogService>();
        services.AddSingleton<IDbMigrator, TarantoolSchemaProbe>();

        return services;
    }
}
