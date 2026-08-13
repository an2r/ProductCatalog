using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApiApp.Contracts;
using WebApiApp.Services;

namespace WebApiApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [Authorize(Roles = WebApiApp.Auth.AppRoles.Administrator)]
    [HttpPatch("users/{userId:int}/block")]
    public async Task<IActionResult> BlockUser(
        int userId,
        [FromBody] BlockUserRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await authService.BlockUserAsync(userId, request.IsBlocked, cancellationToken);
        return updated ? Ok(new { userId, isBlocked = request.IsBlocked }) : NotFound();
    }

    [Authorize(Roles = WebApiApp.Auth.AppRoles.Administrator)]
    [HttpPatch("users/{userId:int}/unblock")]
    public async Task<IActionResult> UnblockUser(int userId, CancellationToken cancellationToken)
    {
        var updated = await authService.UnblockUserAsync(userId, cancellationToken);
        return updated ? Ok(new { userId, isBlocked = false }) : NotFound();
    }

    [Authorize(Roles = WebApiApp.Auth.AppRoles.Administrator)]
    [HttpDelete("users/{userId:int}")]
    public async Task<IActionResult> DeleteUser(int userId, CancellationToken cancellationToken)
    {
        var deleted = await authService.DeleteUserAsync(userId, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [Authorize(Roles = WebApiApp.Auth.AppRoles.Administrator)]
    [HttpPut("users/{userId:int}/password")]
    public async Task<IActionResult> ChangeUserPassword(
        int userId,
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var changed = await authService.ChangePasswordAsync(userId, request.NewPassword, cancellationToken);
        return changed ? Ok(new { userId, message = "Пароль успешно изменён." }) : NotFound();
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request.Login, request.Password, cancellationToken);

        return result is null
            ? Unauthorized(new { message = "Неверный логин или пароль." })
            : Ok(ToResponse(result));
    }

    [Authorize(Roles = WebApiApp.Auth.AppRoles.Administrator)]
    [HttpPost("users")]
    public async Task<ActionResult<UserProfile>> CreateUser(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Логин и пароль обязательны." });
        }

        if (request.Password.Length < 6)
        {
            return BadRequest(new { message = "Пароль должен содержать не менее 6 символов." });
        }

        if (!string.IsNullOrWhiteSpace(request.Role) && !WebApiApp.Auth.AppRoles.All.Contains(request.Role))
        {
            return BadRequest(new
            {
                message = $"Недопустимая роль. Допустимые значения: {string.Join(", ", WebApiApp.Auth.AppRoles.All)}."
            });
        }

        var result = await authService.CreateUserAsync(request.Login, request.Password, request.Role, cancellationToken);

        return result is null
            ? Conflict(new { message = "Пользователь с таким логином уже существует." })
            : Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserProfile>> GetProfile(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var profile = await authService.GetProfileAsync(userId, cancellationToken);

        return profile is null ? NotFound() : Ok(profile);
    }

    private static AuthResponse ToResponse(AuthResult result) =>
        new(result.Token, result.UserId, result.Login, result.Role);
}

