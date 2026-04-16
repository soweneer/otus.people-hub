using Microsoft.Extensions.DependencyInjection;

namespace PeopleHub.Shared
{
    public static class Bootstrapper
    {
        public static IServiceCollection AddPeopleHubShared(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg => cfg.AddMaps(typeof(Bootstrapper).Assembly));

            return services;
        }
    }
}
