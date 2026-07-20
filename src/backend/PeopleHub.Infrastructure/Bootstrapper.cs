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
using PeopleHub.Infrastructure.Queries;
using PeopleHub.Infrastructure.Repositories;
using StackExchange.Redis;

namespace PeopleHub.Infrastructure;

public static class Bootstrapper
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
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
        services.Decorate<IFeedService, CachingFeedServiceDecorator>();
        services.Decorate<IPostService, FeedCacheCorrectingPostServiceDecorator>();
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")));
        services.AddScoped<IFeedCacheService, RedisFeedCacheService>();
        services.AddSingleton<FeedCacheCorrectionQueue>();
        services.AddHostedService<FeedCacheCorrectionWorker>();

        return services;
    }
}
