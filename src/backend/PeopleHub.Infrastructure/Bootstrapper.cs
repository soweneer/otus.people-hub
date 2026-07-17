using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using PeopleHub.Application.Abstractions;
using PeopleHub.Domain.Repositories;
using PeopleHub.Domain.Services;
using PeopleHub.Infrastructure.Db;
using PeopleHub.Infrastructure.Helpers;
using PeopleHub.Infrastructure.Queries;
using PeopleHub.Infrastructure.Repositories;

namespace PeopleHub.Infrastructure
{
    public static class Bootstrapper
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string dbConnectionString)
        {
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

            return services;
        }
    }
}
