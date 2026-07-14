using CleanApiStarter.Application.Auth;

namespace CleanApiStarter.Application.Abstractions;

public interface IRefreshTokenGenerator
{
    RefreshTokenResult Generate();
    string Hash(string token);
}
