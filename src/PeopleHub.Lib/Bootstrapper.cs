using Microsoft.Extensions.DependencyInjection;

namespace PeopleHub.Lib
{
    public static class Bootstrapper
    {
        public static IServiceCollection AddPeopleHubLib(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Bootstrapper).Assembly));
            services.AddAutoMapper(cfg => cfg.AddMaps(typeof(Bootstrapper).Assembly));

            return services;
        }
    }
}
