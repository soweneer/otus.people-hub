using Microsoft.Extensions.DependencyInjection;
using PeopleHub.Domain.Repositories;
using PeopleHub.Domain.Services;
using PeopleHub.Infrastructure.Db;
using PeopleHub.Infrastructure.Repositories;

namespace PeopleHub.Infrastructure
{
    public static class Bootstrapper
    {
        public static IServiceCollection AddPeopleHubInfrastructure(this IServiceCollection services, string dbConnectionString)
        {
            services.AddScoped(_ => new DbClient(dbConnectionString));
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IFriendRequestRepository, FriendRequestRequestRepository>();
            services.AddScoped<IPersonRepository, PersonRepository>();
            services.AddScoped<IAdminRepository, AdminRepository>();

            return services;
        }
    }
}
