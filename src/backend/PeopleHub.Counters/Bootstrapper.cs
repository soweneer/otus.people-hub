using PeopleHub.Counters.Messaging;
using PeopleHub.Counters.Services;
using PeopleHub.Counters.Storage;
using StackExchange.Redis;

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

        return services;
    }
}
