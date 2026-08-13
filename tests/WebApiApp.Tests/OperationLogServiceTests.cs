using Microsoft.EntityFrameworkCore;
using WebApiApp.Services;

namespace WebApiApp.Tests;

public sealed class OperationLogServiceTests
{
    [Fact]
    public async Task LogAsync_StoresCyrillicPayloadWithoutUnicodeEscapes()
    {
        await using var dbContext = TestDatabase.CreateContext();
        var service = TestDatabase.CreateOperationLogService(dbContext);

        await service.LogAsync(
            entityType: "Category",
            entityId: 1,
            operation: "Create",
            payload: new { CategoryName = "Алкоголь" },
            httpContext: null);

        var payload = await dbContext.OperationLogs
            .Select(log => log.Payload)
            .SingleAsync();

        Assert.Equal("{\"CategoryName\":\"Алкоголь\"}", payload);
        Assert.DoesNotContain("\\u", payload, StringComparison.OrdinalIgnoreCase);
    }
}
