using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PeopleHub.Domain.Repositories;
using PeopleHub.Infrastructure.Db;
using PeopleHub.Infrastructure.Repositories;

namespace PeopleHub.Infrastructure
{
    public static class Bootstrapper
    {
        public static IServiceCollection AddPeopleHubInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var dbConnectionString = configuration.GetConnectionString("PostgreSql");
            if (string.IsNullOrEmpty(dbConnectionString))
                throw new MissingMemberException("Connection string is absent");
            
            services.AddScoped(_ => new DbClient(dbConnectionString));
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IFriendRequestRepository, FriendRequestRepository>();
            services.AddScoped<IPersonRepository, PersonRepository>();

            return services;
        }
    }
}
