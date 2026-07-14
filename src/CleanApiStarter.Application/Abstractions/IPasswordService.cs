using CleanApiStarter.Domain.Entities;

namespace CleanApiStarter.Application.Abstractions;

public interface IPasswordService
{
    string Hash(User user, string password);
    bool Verify(User user, string passwordHash, string providedPassword);
}
