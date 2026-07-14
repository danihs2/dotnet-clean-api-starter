using CleanApiStarter.Application.Auth;
using CleanApiStarter.Domain.Entities;

namespace CleanApiStarter.Application.Abstractions;

public interface IJwtTokenGenerator
{
    TokenResult Generate(User user);
}
