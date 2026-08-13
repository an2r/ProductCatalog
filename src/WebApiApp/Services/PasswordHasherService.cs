using Microsoft.AspNetCore.Identity;
using WebApiApp.Models;

namespace WebApiApp.Services;

public interface IPasswordHasherService
{
    string HashPassword(User user, string password);

    bool VerifyPassword(User user, string password, string passwordHash);
}

public class PasswordHasherService : IPasswordHasherService
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public string HashPassword(User user, string password) =>
        _passwordHasher.HashPassword(user, password);

    public bool VerifyPassword(User user, string password, string passwordHash) =>
        _passwordHasher.VerifyHashedPassword(user, passwordHash, password)
            is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
}
