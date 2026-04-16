using Microsoft.Extensions.DependencyInjection;

namespace PeopleHub.Domain
{
    public static class Bootstrapper
    {
        public static IServiceCollection AddPeopleHubDomain(this IServiceCollection services)
        {
            return services;
        }
    }
}
