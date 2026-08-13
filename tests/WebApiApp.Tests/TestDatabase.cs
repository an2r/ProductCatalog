using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WebApiApp.Data;
using WebApiApp.Services;

namespace WebApiApp.Tests;

internal static class TestDatabase
{
    public static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new AppDbContext(options);
    }

    public static IOperationLogService CreateOperationLogService(AppDbContext dbContext) =>
        new OperationLogService(dbContext, new HttpContextAccessor());
}
