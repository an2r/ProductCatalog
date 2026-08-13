using Microsoft.EntityFrameworkCore;
using WebApiApp.Application.Categories;
using WebApiApp.Application.Exceptions;
using WebApiApp.Contracts;
using WebApiApp.Models;

namespace WebApiApp.Tests;

public sealed class CategoryApplicationServiceTests
{
    [Fact]
    public async Task CreateCategoryAsync_WithDuplicateName_ThrowsConflictException()
    {
        await using var dbContext = TestDatabase.CreateContext();
        dbContext.Categories.Add(new Category { CategoryId = 7, CategoryName = "Компьютеры" });
        await dbContext.SaveChangesAsync();

        var service = new CategoryApplicationService(
            dbContext,
            TestDatabase.CreateOperationLogService(dbContext));

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateCategoryAsync(new CreateCategoryRequest("  Компьютеры  "), CancellationToken.None));

        Assert.Equal("Категория с таким названием уже существует.", exception.Message);
    }

    [Fact]
    public async Task DeleteCategoryAsync_DeletesRelatedProducts_AndWritesAuditRecords()
    {
        await using var dbContext = TestDatabase.CreateContext();
        var category = new Category { CategoryId = 10, CategoryName = "Ноутбуки" };
        dbContext.Categories.Add(category);
        dbContext.ProductCatalogs.AddRange(
            new ProductCatalog
            {
                ProductId = 101,
                ProductName = "Первый",
                ProductCathegory = category.CategoryId,
                Category = category,
                ProductCost = 100m
            },
            new ProductCatalog
            {
                ProductId = 102,
                ProductName = "Второй",
                ProductCathegory = category.CategoryId,
                Category = category,
                ProductCost = 200m
            });
        await dbContext.SaveChangesAsync();

        var service = new CategoryApplicationService(
            dbContext,
            TestDatabase.CreateOperationLogService(dbContext));

        var deleted = await service.DeleteCategoryAsync(category.CategoryId, CancellationToken.None);

        Assert.True(deleted);
        Assert.False(await dbContext.Categories.AnyAsync(item => item.CategoryId == category.CategoryId));
        Assert.Empty(await dbContext.ProductCatalogs.Where(item => item.ProductCathegory == category.CategoryId).ToListAsync());

        var auditRecords = await dbContext.OperationLogs
            .Where(item => item.Operation == "Delete")
            .ToListAsync();

        Assert.Equal(3, auditRecords.Count);
        Assert.Equal(2, auditRecords.Count(item => item.EntityType == nameof(ProductCatalog)));
        Assert.Single(auditRecords, item => item.EntityType == nameof(Category));
    }
}
