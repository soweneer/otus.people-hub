using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using PeopleHub.Application.Abstractions;
using PeopleHub.Application.Services;
using PeopleHub.Domain.Repositories;
using PeopleHub.Domain.Services;
using PeopleHub.Infrastructure.Db;
using PeopleHub.Infrastructure.Decorators;
using PeopleHub.Infrastructure.Helpers;
using PeopleHub.Infrastructure.Queries;
using PeopleHub.Infrastructure.Repositories;

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
        
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (string.IsNullOrEmpty(redisConnectionString))
            throw new MissingMemberException("Redis connection string is absent");
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "people-hub:";
        });
        services.Decorate<IFeedService, CachingFeedServiceDecorator>();

        return services;
    }
}
