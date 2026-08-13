namespace WebApiApp.Contracts;

public record LoginRequest(string Login, string Password);

public record CreateUserRequest(string Login, string Password, string? Role = null);

public record AuthResponse(string Token, int UserId, string Login, string Role);

public record BlockUserRequest(bool IsBlocked);

public record ChangePasswordRequest(string NewPassword);
