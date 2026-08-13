using WebApiApp.Application.Catalog;
using WebApiApp.Contracts;
using WebApiApp.Models;
using WebApiApp.Services;

namespace WebApiApp.Tests;

public sealed class CatalogApplicationServiceTests
{
    [Fact]
    public async Task GetCatalogAsync_AppliesSearchCategoryAndPriceFilters()
    {
        await using var dbContext = TestDatabase.CreateContext();
        var computers = new Category { CategoryId = 1, CategoryName = "Компьютеры" };
        var accessories = new Category { CategoryId = 2, CategoryName = "Аксессуары" };
        dbContext.Categories.AddRange(computers, accessories);
        dbContext.ProductCatalogs.AddRange(
            new ProductCatalog
            {
                ProductId = 1,
                ProductName = "Игровой ноутбук",
                ProductCathegory = computers.CategoryId,
                Category = computers,
                ProductDescription = "Мощный компьютер",
                ProductCost = 2500m
            },
            new ProductCatalog
            {
                ProductId = 2,
                ProductName = "Офисный ноутбук",
                ProductCathegory = computers.CategoryId,
                Category = computers,
                ProductDescription = "Компактный компьютер",
                ProductCost = 900m
            },
            new ProductCatalog
            {
                ProductId = 3,
                ProductName = "Мышь",
                ProductCathegory = accessories.CategoryId,
                Category = accessories,
                ProductDescription = "Игровая мышь",
                ProductCost = 100m
            });
        await dbContext.SaveChangesAsync();

        var service = new CatalogApplicationService(
            dbContext,
            TestDatabase.CreateOperationLogService(dbContext),
            new StubCatalogPdfExportService());

        var items = await service.GetCatalogAsync(
            search: "ноутбук",
            categoryName: "Компьютеры",
            minCost: 1000m,
            maxCost: 3000m,
            cancellationToken: CancellationToken.None);

        var item = Assert.Single(items);
        Assert.Equal(1, item.ProductId);
        Assert.Equal("Игровой ноутбук", item.ProductName);
        Assert.Equal("Компьютеры", item.ProductCathegory);
    }

    private sealed class StubCatalogPdfExportService : ICatalogPdfExportService
    {
        public byte[] GeneratePdf(
            IEnumerable<CatalogItemResponse> products,
            string? search,
            string? categoryName,
            decimal? minCost,
            decimal? maxCost) => [];
    }
}
