using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using WebApiApp.Data;
using WebApiApp.Models;

namespace WebApiApp.Services;

public interface IOperationLogService
{
    Task LogAsync(string entityType, int entityId, string operation, object? payload, HttpContext? httpContext, CancellationToken cancellationToken = default);
}

public class OperationLogService(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor) : IOperationLogService
{
    private static readonly JsonSerializerOptions PayloadJsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public async Task LogAsync(
        string entityType,
        int entityId,
        string operation,
        object? payload,
        HttpContext? httpContext,
        CancellationToken cancellationToken = default)
    {
        var context = httpContext ?? httpContextAccessor.HttpContext;

        var userName = context?.User?.Identity?.IsAuthenticated == true
            ? context.User.FindFirstValue(ClaimTypes.Name)
            ?? context.User.Identity.Name
            : null;

        var log = new OperationLog
        {
            EntityType = entityType,
            EntityId = entityId,
            Operation = operation,
            UserName = userName,
            Payload = payload is null ? null : JsonSerializer.Serialize(payload, PayloadJsonOptions)
        };

        dbContext.OperationLogs.Add(log);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
