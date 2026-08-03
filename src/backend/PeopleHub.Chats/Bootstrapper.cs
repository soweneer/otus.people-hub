using Npgsql;
using PeopleHub.Chats.Db;
using PeopleHub.Chats.Messaging;
using PeopleHub.Chats.Outbox;
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

        services.AddSingleton(new NpgsqlDataSourceBuilder(connectionString).Build());
        services.AddScoped<DbClient>();
        services.AddScoped<IDialogRepository, DialogRepository>();
        services.AddScoped<IDialogService, DialogService>();
        services.AddScoped<IDbMigrator, DbMigrator>();

        services.AddOutbox(configuration);

        return services;
    }

    private static IServiceCollection AddOutbox(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitConnectionString = configuration.GetConnectionString("RabbitMq");
        if (string.IsNullOrEmpty(rabbitConnectionString))
        {
            throw new MissingMemberException("RabbitMq connection string is absent");
        }

        services.AddSingleton(new RabbitMqOptions
        {
            ConnectionString = rabbitConnectionString,
            ClientName = $"people-hub-chats-{Environment.MachineName}"
        });
        services.AddSingleton<RabbitMqConnection>();
        services.AddSingleton<CountersEventPublisher>();

        services.AddSingleton(configuration.GetSection("Outbox").Get<OutboxOptions>() ?? new OutboxOptions());
        services.AddScoped<OutboxDispatcher>();
        services.AddHostedService<OutboxPublisherWorker>();

        return services;
    }
}
