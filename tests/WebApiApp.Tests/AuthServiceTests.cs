using WebApiApp.Models;
using WebApiApp.Services;

namespace WebApiApp.Tests;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task LoginAsync_WithWrongPassword_ReturnsNoToken()
    {
        await using var dbContext = TestDatabase.CreateContext();
        var passwordHasher = new PasswordHasherService();
        var role = new Role { RoleId = 1, RoleName = "User" };
        var user = new User
        {
            UserId = 1,
            UserLogin = "test-user",
            UserRole = role.RoleId,
            Role = role
        };
        user.PasswordHash = passwordHasher.HashPassword(user, "Correct-password-1");

        dbContext.Roles.Add(role);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var authService = new AuthService(
            dbContext,
            passwordHasher,
            new StubTokenService(),
            TestDatabase.CreateOperationLogService(dbContext));

        var result = await authService.LoginAsync("test-user", "Wrong-password", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task UserAccessService_BlockedUser_IsRejectedByJwtAccessValidation()
    {
        await using var dbContext = TestDatabase.CreateContext();
        dbContext.Users.Add(new User
        {
            UserId = 42,
            UserLogin = "blocked-user",
            UserRole = 1,
            PasswordHash = "unused",
            IsBlocked = true
        });
        await dbContext.SaveChangesAsync();

        var userAccessService = new UserAccessService(dbContext);

        var isActive = await userAccessService.IsActiveAsync(42, CancellationToken.None);

        Assert.False(isActive);
    }

    private sealed class StubTokenService : ITokenService
    {
        public string CreateToken(User user) => "test-token";
    }
}
