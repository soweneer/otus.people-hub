using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using PeopleHub.Application.Abstractions;
using PeopleHub.Application.Services;
using PeopleHub.Domain.Repositories;
using PeopleHub.Domain.Services;
using PeopleHub.Infrastructure.Caching;
using PeopleHub.Infrastructure.Db;
using PeopleHub.Infrastructure.Helpers;
using PeopleHub.Infrastructure.Messaging;
using PeopleHub.Infrastructure.Queries;
using PeopleHub.Infrastructure.Repositories;
using StackExchange.Redis;

namespace PeopleHub.Infrastructure;

public static class Bootstrapper
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddInfrastructure(IConfiguration configuration, string clientName = "people-hub")
        {
            var dbConnectionString = configuration.GetConnectionString("PostgreSql");
            if (string.IsNullOrEmpty(dbConnectionString))
                throw new MissingMemberException("Connection string is absent");
            services.AddSingleton(new NpgsqlDataSourceBuilder(dbConnectionString).BuildMultiHost());
            services.AddScoped(sp => new DbClient(sp.GetRequiredService<NpgsqlMultiHostDataSource>()));

            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IFriendRequestRepository, FriendRequestRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPostRepository, PostRepository>();

            services.AddScoped<IUserQueries, UserQueries>();
            services.AddScoped<IFriendQueries, FriendQueries>();
            services.AddScoped<IFeedRepository, FeedRepository>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IDbMigrator, DbMigrator>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
        
            FeedCacheMetrics.Publish();
            services.Configure<FeatureFlagsOptions>(configuration.GetSection("FeatureFlags"));
            var redis = ConnectRedis(configuration);
            services.AddSingleton(redis);
            services.AddCaching();
            services.AddMessaging(configuration, clientName);
            services.AddSharedDataProtection(redis);

            return services;
        }

        public IServiceCollection AddSharedDataProtection(IConnectionMultiplexer redis)
        {
            services.AddDataProtection()
                .SetApplicationName("PeopleHub")
                .PersistKeysToStackExchangeRedis(redis, "people-hub:data-protection-keys");

            return services;
        }

        public IServiceCollection AddMessaging(IConfiguration configuration, string clientName = "people-hub")
        {
            var rabbitConnectionString = configuration.GetConnectionString("RabbitMq");
            if (string.IsNullOrEmpty(rabbitConnectionString))
                throw new MissingMemberException("RabbitMq connection string is absent");

            services.AddSingleton(new RabbitMqOptions
            {
                ConnectionString = rabbitConnectionString,
                ClientName = $"{clientName}-{Environment.MachineName}"
            });
            services.AddSingleton<RabbitMqConnection>();
            services.AddSingleton<IFeedEventPublisher, RabbitFeedEventPublisher>();

            return services;
        }

        private static IConnectionMultiplexer ConnectRedis(IConfiguration configuration)
        {
            var redisOptions = ConfigurationOptions.Parse(configuration.GetConnectionString("Redis")!);
            redisOptions.AbortOnConnectFail = false;

            return ConnectionMultiplexer.Connect(redisOptions);
        }

        private void AddCaching()
        {
            services.Decorate<IFeedService, CachingFeedServiceDecorator>();
            services.Decorate<IPostService, PostEventPublishingDecorator>();
            services.AddScoped<IFeedCacheService, RedisFeedCacheService>();
        }
    }
}
