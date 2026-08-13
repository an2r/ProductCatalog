namespace WebApiApp.Models;

public class User
{
    public int UserId { get; set; }

    public string UserLogin { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int UserRole { get; set; }

    public bool IsBlocked { get; set; }

    public Role Role { get; set; } = null!;
}
