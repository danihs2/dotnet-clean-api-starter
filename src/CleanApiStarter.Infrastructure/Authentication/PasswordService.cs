using CleanApiStarter.Application.Abstractions;
using CleanApiStarter.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace CleanApiStarter.Infrastructure.Authentication;

public sealed class PasswordService(IPasswordHasher<User> hasher) : IPasswordService
{
    public string Hash(User user, string password) => hasher.HashPassword(user, password);

    public bool Verify(User user, string passwordHash, string providedPassword) =>
        hasher.VerifyHashedPassword(user, passwordHash, providedPassword) != PasswordVerificationResult.Failed;
}
