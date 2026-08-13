using Microsoft.EntityFrameworkCore;
using WebApiApp.Contracts;
using WebApiApp.Data;
using WebApiApp.Models;
using WebApiApp.Services;

namespace WebApiApp.Application.Catalog;

public interface ICatalogApplicationService
{
    Task<IReadOnlyList<CatalogItemResponse>> GetCatalogAsync(
        string? search,
        string? categoryName,
        decimal? minCost,
        decimal? maxCost,
        CancellationToken cancellationToken);

    Task<byte[]> ExportPdfAsync(
        string? search,
        string? categoryName,
        decimal? minCost,
        decimal? maxCost,
        CancellationToken cancellationToken);

    Task<CatalogItemResponse> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken);

    Task<CatalogItemResponse?> UpdateProductAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken);

    Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken);
}

public sealed class CatalogApplicationService(
    AppDbContext dbContext,
    IOperationLogService operationLogService,
    ICatalogPdfExportService catalogPdfExportService) : ICatalogApplicationService
{
    public async Task<IReadOnlyList<CatalogItemResponse>> GetCatalogAsync(
        string? search,
        string? categoryName,
        decimal? minCost,
        decimal? maxCost,
        CancellationToken cancellationToken) =>
        await ProjectToCatalogItems(BuildFilteredQuery(search, categoryName, minCost, maxCost))
            .OrderBy(item => item.ProductId)
            .ToListAsync(cancellationToken);

    public async Task<byte[]> ExportPdfAsync(
        string? search,
        string? categoryName,
        decimal? minCost,
        decimal? maxCost,
        CancellationToken cancellationToken)
    {
        var items = await GetCatalogAsync(search, categoryName, minCost, maxCost, cancellationToken);
        return catalogPdfExportService.GeneratePdf(items, search, categoryName, minCost, maxCost);
    }

    public async Task<CatalogItemResponse> CreateProductAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        await EnsureCategoryExistsAsync(request.ProductCathegory, cancellationToken);

        var product = new ProductCatalog
        {
            ProductName = request.ProductName,
            ProductCathegory = request.ProductCathegory,
            ProductDescription = request.ProductDescription,
            ProductCost = request.ProductCost,
            ProductNote = request.ProductNote,
            ProductNoteSpecial = request.ProductNoteSpecial
        };

        dbContext.ProductCatalogs.Add(product);
        await dbContext.SaveChangesAsync(cancellationToken);

        await operationLogService.LogAsync(
            nameof(ProductCatalog),
            product.ProductId,
            "Create",
            new { product.ProductName, product.ProductCathegory, product.ProductDescription, product.ProductCost },
            httpContext: null,
            cancellationToken: cancellationToken);

        return await ToResponseAsync(product, cancellationToken);
    }

    public async Task<CatalogItemResponse?> UpdateProductAsync(
        int id,
        UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await dbContext.ProductCatalogs
            .FirstOrDefaultAsync(item => item.ProductId == id, cancellationToken);

        if (product is null)
        {
            return null;
        }

        await EnsureCategoryExistsAsync(request.ProductCathegory, cancellationToken);

        product.ProductName = request.ProductName;
        product.ProductCathegory = request.ProductCathegory;
        product.ProductDescription = request.ProductDescription;
        product.ProductCost = request.ProductCost;
        product.ProductNote = request.ProductNote;
        product.ProductNoteSpecial = request.ProductNoteSpecial;

        await dbContext.SaveChangesAsync(cancellationToken);

        await operationLogService.LogAsync(
            nameof(ProductCatalog),
            product.ProductId,
            "Update",
            new { product.ProductId, product.ProductName, product.ProductCathegory, product.ProductDescription, product.ProductCost },
            httpContext: null,
            cancellationToken: cancellationToken);

        return await ToResponseAsync(product, cancellationToken);
    }

    public async Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken)
    {
        var product = await dbContext.ProductCatalogs
            .FirstOrDefaultAsync(item => item.ProductId == id, cancellationToken);

        if (product is null)
        {
            return false;
        }

        dbContext.ProductCatalogs.Remove(product);
        await dbContext.SaveChangesAsync(cancellationToken);

        await operationLogService.LogAsync(
            nameof(ProductCatalog),
            product.ProductId,
            "Delete",
            new { product.ProductId, product.ProductName, product.ProductCathegory },
            httpContext: null,
            cancellationToken: cancellationToken);

        return true;
    }

    private IQueryable<ProductCatalog> BuildFilteredQuery(
        string? search,
        string? categoryName,
        decimal? minCost,
        decimal? maxCost)
    {
        var query = dbContext.ProductCatalogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            query = query.Where(product =>
                (product.ProductName != null && product.ProductName.Contains(normalizedSearch)) ||
                (product.ProductDescription != null && product.ProductDescription.Contains(normalizedSearch)) ||
                (product.ProductNote != null && product.ProductNote.Contains(normalizedSearch)) ||
                (product.ProductNoteSpecial != null && product.ProductNoteSpecial.Contains(normalizedSearch)));
        }

        if (!string.IsNullOrWhiteSpace(categoryName))
        {
            var normalizedCategoryName = categoryName.Trim();
            query = query.Where(product => product.Category != null && product.Category.CategoryName.Contains(normalizedCategoryName));
        }

        if (minCost.HasValue)
        {
            query = query.Where(product => product.ProductCost >= minCost.Value);
        }

        if (maxCost.HasValue)
        {
            query = query.Where(product => product.ProductCost <= maxCost.Value);
        }

        return query;
    }

    private async Task EnsureCategoryExistsAsync(int? categoryId, CancellationToken cancellationToken)
    {
        if (categoryId is null)
        {
            return;
        }

        var categoryExists = await dbContext.Categories
            .AnyAsync(category => category.CategoryId == categoryId.Value, cancellationToken);

        if (!categoryExists)
        {
            throw new ArgumentException("Указанная категория не существует.", nameof(categoryId));
        }
    }

    private async Task<CatalogItemResponse> ToResponseAsync(ProductCatalog product, CancellationToken cancellationToken)
    {
        var categoryName = product.ProductCathegory is null
            ? null
            : await dbContext.Categories
                .Where(category => category.CategoryId == product.ProductCathegory.Value)
                .Select(category => category.CategoryName)
                .FirstOrDefaultAsync(cancellationToken);

        return new CatalogItemResponse(
            product.ProductId,
            product.ProductName,
            categoryName,
            product.ProductDescription,
            product.ProductCost,
            product.ProductNote,
            product.ProductNoteSpecial);
    }

    private static IQueryable<CatalogItemResponse> ProjectToCatalogItems(IQueryable<ProductCatalog> query) =>
        query.Select(product => new CatalogItemResponse(
            product.ProductId,
            product.ProductName,
            product.Category == null ? null : product.Category.CategoryName,
            product.ProductDescription,
            product.ProductCost,
            product.ProductNote,
            product.ProductNoteSpecial));
}
