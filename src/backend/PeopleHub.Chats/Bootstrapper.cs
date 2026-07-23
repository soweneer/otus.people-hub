using Npgsql;
using PeopleHub.Chats.Db;
using PeopleHub.Chats.Repositories;
using PeopleHub.Chats.Services;

namespace PeopleHub.Chats;

public static class Bootstrapper
{
    public static IServiceCollection AddChats(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSql");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new MissingMemberException("Connection string is absent");
        }

        services.Configure<CitusOptions>(configuration.GetSection("Citus"));

        services.AddSingleton(new NpgsqlDataSourceBuilder(connectionString).Build());
        services.AddScoped<DbClient>();
        services.AddScoped<IDialogRepository, DialogRepository>();
        services.AddScoped<IDialogService, DialogService>();
        services.AddScoped<IDbMigrator, DbMigrator>();

        return services;
    }
}
