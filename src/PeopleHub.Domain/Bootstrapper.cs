using Microsoft.Extensions.DependencyInjection;

namespace PeopleHub.Domain
{
    public static class Bootstrapper
    {
        public static IServiceCollection AddPeopleHubDomain(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Bootstrapper).Assembly));

            return services;
        }
    }
}
