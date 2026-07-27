using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace PeopleHub.Feed.Auth;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddFeedAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
            })
            .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSection["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSection["Audience"],
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"] ?? string.Empty))
            });

        services.AddAuthorizationBuilder()
            .SetDefaultPolicy(new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .RequireClaim(ClaimTypes.NameIdentifier)
                .Build());

        return services;
    }
}
