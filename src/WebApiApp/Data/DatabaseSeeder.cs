using Microsoft.EntityFrameworkCore;
using WebApiApp.Auth;
using WebApiApp.Models;

namespace WebApiApp.Data;

public static class DatabaseSeeder
{
    private static readonly (string Name, string Description)[] DefaultRoles =
    [
        (AppRoles.Administrator, "Полный доступ к системе"),
        (AppRoles.User, "Базовый доступ"),
        (AppRoles.AdvancedUser, "Расширенный доступ")
    ];

    public static async Task SeedAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        foreach (var (name, description) in DefaultRoles)
        {
            var exists = await dbContext.Roles
                .AnyAsync(r => r.RoleName == name, cancellationToken);

            if (!exists)
            {
                dbContext.Roles.Add(new Role
                {
                    RoleName = name,
                    RoleDescription = description
                });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
