using PeopleHub.Counters.Messaging;
using PeopleHub.Counters.Services;
using PeopleHub.Counters.Storage;
using StackExchange.Redis;
using ChatsDialogs = PeopleHub.Chats.Grpc.Dialogs;

namespace PeopleHub.Counters;

public static class Bootstrapper
{
    public static IServiceCollection AddCounters(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (string.IsNullOrEmpty(redisConnectionString))
        {
            throw new MissingMemberException("Redis connection string is absent");
        }

        var redisOptions = ConfigurationOptions.Parse(redisConnectionString);
        redisOptions.AbortOnConnectFail = false;

        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisOptions));
        services.AddSingleton<ICounterStore, RedisCounterStore>();

        var rabbitConnectionString = configuration.GetConnectionString("RabbitMq");
        if (string.IsNullOrEmpty(rabbitConnectionString))
        {
            throw new MissingMemberException("RabbitMq connection string is absent");
        }

        services.AddSingleton(new RabbitMqOptions
        {
            ConnectionString = rabbitConnectionString,
            ClientName = $"people-hub-counters-{Environment.MachineName}"
        });
        services.AddSingleton<RabbitMqConnection>();
        services.AddSingleton<CounterNotificationPublisher>();
        services.AddHostedService<CounterApplyWorker>();

        services.AddReconciler(configuration);

        return services;
    }

    private static IServiceCollection AddReconciler(this IServiceCollection services, IConfiguration configuration)
    {
        var address = configuration["ChatsService:Address"]
                      ?? throw new MissingMemberException("ChatsService:Address configuration is absent");

        services.AddSingleton(configuration.GetSection("Reconciler").Get<ReconcilerOptions>() ?? new ReconcilerOptions());
        services.AddGrpcClient<ChatsDialogs.DialogsClient>(options => options.Address = new Uri(address));
        services.AddSingleton<IDialogTruthSource, ChatsTruthSource>();
        services.AddSingleton<CounterReconciler>();
        services.AddHostedService<ReconcilerWorker>();

        return services;
    }
}
