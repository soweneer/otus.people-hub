using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using PeopleHub.Domain.Repositories;
using PeopleHub.Domain.Services;
using PeopleHub.Infrastructure.Db;
using PeopleHub.Infrastructure.Helpers;
using PeopleHub.Infrastructure.Repositories;

namespace PeopleHub.Infrastructure
{
    public static class Bootstrapper
    {
        public static IServiceCollection AddPeopleHubInfrastructure(this IServiceCollection services, string dbConnectionString)
        {
            // Строка подключения перечисляет все ноды кластера (мастер + реплики);
            // multi-host data source сам определяет, кто из них primary, а кто standby
            services.AddSingleton(new NpgsqlDataSourceBuilder(dbConnectionString).BuildMultiHost());
            services.AddScoped(sp => new DbClient(sp.GetRequiredService<NpgsqlMultiHostDataSource>()));
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IFriendRequestRepository, FriendRequestRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ISearchRepository, SearchRepository>();
            services.AddScoped<IAdminRepository, AdminRepository>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();

            return services;
        }
    }
}
