namespace WebApiApp.Models;

public class Role
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public string? RoleDescription { get; set; }

    public ICollection<User> Users { get; set; } = [];
}
