using Microsoft.EntityFrameworkCore;
using WebApiApp.Auth;
using WebApiApp.Data;
using WebApiApp.Models;

namespace WebApiApp.Services;

public interface IAuthService
{
    Task<AuthResult?> LoginAsync(string login, string password, CancellationToken cancellationToken);

    Task<UserProfile?> CreateUserAsync(string login, string password, string? roleName, CancellationToken cancellationToken);

    Task<UserProfile?> GetProfileAsync(int userId, CancellationToken cancellationToken);

    Task<bool> BlockUserAsync(int userId, bool isBlocked, CancellationToken cancellationToken);

    Task<bool> UnblockUserAsync(int userId, CancellationToken cancellationToken);

    Task<bool> DeleteUserAsync(int userId, CancellationToken cancellationToken);

    Task<bool> ChangePasswordAsync(int userId, string newPassword, CancellationToken cancellationToken);
}

public record AuthResult(string Token, int UserId, string Login, string Role);

public record UserProfile(int UserId, string Login, string Role);

public class AuthService(
    AppDbContext dbContext,
    IPasswordHasherService passwordHasher,
    ITokenService tokenService,
    IOperationLogService operationLogService) : IAuthService
{
    public async Task<AuthResult?> LoginAsync(
        string login,
        string password,
        CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserLogin == login, cancellationToken);

        if (user is null || user.IsBlocked)
        {
            return null;
        }

        if (!passwordHasher.VerifyPassword(user, password, user.PasswordHash))
        {
            return null;
        }

        return CreateAuthResult(user);
    }

    public async Task<UserProfile?> CreateUserAsync(
        string login,
        string password,
        string? roleName,
        CancellationToken cancellationToken)
    {
        if (await dbContext.Users.AnyAsync(u => u.UserLogin == login, cancellationToken))
        {
            return null;
        }

        var selectedRoleName = string.IsNullOrWhiteSpace(roleName) ? AppRoles.User : roleName.Trim();
        var userRole = await dbContext.Roles
            .FirstOrDefaultAsync(r => r.RoleName == selectedRoleName, cancellationToken);

        if (userRole is null)
        {
            throw new InvalidOperationException(
                $"Role '{selectedRoleName}' is not configured in the database.");
        }

        var user = new User
        {
            UserLogin = login,
            UserRole = userRole.RoleId,
            IsBlocked = false
        };

        user.PasswordHash = passwordHasher.HashPassword(user, password);
        user.Role = userRole;

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new UserProfile(user.UserId, user.UserLogin, user.Role.RoleName);
    }

    public async Task<UserProfile?> GetProfileAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        return user is null
            ? null
            : new UserProfile(user.UserId, user.UserLogin, user.Role.RoleName);
    }

    public Task<bool> BlockUserAsync(int userId, bool isBlocked, CancellationToken cancellationToken) =>
        SetUserBlockStateAsync(userId, isBlocked, cancellationToken);

    public Task<bool> UnblockUserAsync(int userId, CancellationToken cancellationToken) =>
        SetUserBlockStateAsync(userId, isBlocked: false, cancellationToken);

    private async Task<bool> SetUserBlockStateAsync(
        int userId,
        bool isBlocked,
        CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(item => item.UserId == userId, cancellationToken);

        if (user is null)
        {
            return false;
        }

        var wasBlocked = user.IsBlocked;
        user.IsBlocked = isBlocked;
        await dbContext.SaveChangesAsync(cancellationToken);

        await operationLogService.LogAsync(
            nameof(User),
            user.UserId,
            isBlocked ? "Block" : "Unblock",
            new { user.UserId, user.UserLogin, WasBlocked = wasBlocked, user.IsBlocked },
            httpContext: null,
            cancellationToken: cancellationToken);

        return true;
    }

    public async Task<bool> DeleteUserAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        if (user is null)
        {
            return false;
        }

        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ChangePasswordAsync(int userId, string newPassword, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
        {
            throw new ArgumentException("Пароль должен содержать не менее 6 символов.", nameof(newPassword));
        }

        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        if (user is null)
        {
            return false;
        }

        user.PasswordHash = passwordHasher.HashPassword(user, newPassword);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private AuthResult CreateAuthResult(User user) =>
        new(tokenService.CreateToken(user), user.UserId, user.UserLogin, user.Role.RoleName);
}
