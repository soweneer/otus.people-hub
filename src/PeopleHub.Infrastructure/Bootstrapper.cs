using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PeopleHub.Infrastructure.Db;

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

            return services;
        }
    }
}
