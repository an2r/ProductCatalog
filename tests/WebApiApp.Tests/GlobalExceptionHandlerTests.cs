using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using WebApiApp.Application.Exceptions;
using WebApiApp.Infrastructure;

namespace WebApiApp.Tests;

public sealed class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task DuplicateCategoryConflict_IsMappedToHttp409ProblemDetails()
    {
        var httpContext = new DefaultHttpContext();
        await using var responseBody = new MemoryStream();
        httpContext.Response.Body = responseBody;

        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);

        var handled = await handler.TryHandleAsync(
            httpContext,
            new ConflictException("Категория с таким названием уже существует."),
            CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status409Conflict, httpContext.Response.StatusCode);
        Assert.Equal("application/problem+json", httpContext.Response.ContentType);

        responseBody.Position = 0;
        using var document = await System.Text.Json.JsonDocument.ParseAsync(responseBody);
        Assert.Equal("Конфликт данных.", document.RootElement.GetProperty("title").GetString());
        Assert.Equal("Категория с таким названием уже существует.", document.RootElement.GetProperty("detail").GetString());
    }
}
