using Microsoft.Extensions.DependencyInjection;
using PeopleHub.Domain.Services;

namespace PeopleHub.Domain
{
    public static class Bootstrapper
    {
        public static IServiceCollection AddPeopleHubDomain(this IServiceCollection services)
        {
            services.AddScoped<IFriendRequestService, FriendRequestService>();
            services.AddScoped<IPersonService, PersonService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<ISearchService, SearchService>();

            return services;
        }
    }
}
