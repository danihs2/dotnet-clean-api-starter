namespace CleanApiStarter.Domain.Constants;

public static class AppRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string User = "User";

    public static readonly IReadOnlyCollection<string> All = [SuperAdmin, User];
}
