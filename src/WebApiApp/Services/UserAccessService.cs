using Microsoft.EntityFrameworkCore;
using WebApiApp.Data;

namespace WebApiApp.Services;

public interface IUserAccessService
{
    Task<bool> IsActiveAsync(int userId, CancellationToken cancellationToken);
}

public sealed class UserAccessService(AppDbContext dbContext) : IUserAccessService
{
    public async Task<bool> IsActiveAsync(int userId, CancellationToken cancellationToken) =>
        await dbContext.Users
            .AsNoTracking()
            .AnyAsync(user => user.UserId == userId && !user.IsBlocked, cancellationToken);
}
