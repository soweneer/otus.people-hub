using Microsoft.Extensions.DependencyInjection;
using PeopleHub.Application.Services;

namespace PeopleHub.Application
{
    public static class Bootstrapper
    {
        public static IServiceCollection AddPeopleHubApplication(this IServiceCollection services)
        {
            services.AddScoped<IFriendRequestService, FriendRequestService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IPostService, PostService>();
            services.AddScoped<IFeedService, FeedService>();

            return services;
        }
    }
}
