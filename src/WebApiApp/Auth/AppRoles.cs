namespace WebApiApp.Auth;

public static class AppRoles
{
    public const string Administrator = "Administrator";
    public const string User = "User";
    public const string AdvancedUser = "AdvancedUser";

    public static readonly IReadOnlyList<string> All =
    [
        Administrator,
        User,
        AdvancedUser
    ];
}
