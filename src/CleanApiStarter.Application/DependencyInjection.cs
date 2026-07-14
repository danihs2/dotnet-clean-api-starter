using CleanApiStarter.Application.Auth;
using CleanApiStarter.Application.Users;
using Microsoft.Extensions.DependencyInjection;

namespace CleanApiStarter.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        return services;
    }
}
