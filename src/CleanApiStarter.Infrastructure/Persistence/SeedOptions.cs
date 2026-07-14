namespace CleanApiStarter.Infrastructure.Persistence;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public string SuperAdminEmail { get; init; } = "admin@example.com";
    public string SuperAdminPassword { get; init; } = "ChangeMe123!";
    public string UserEmail { get; init; } = "user@example.com";
    public string UserPassword { get; init; } = "ChangeMe123!";
}
