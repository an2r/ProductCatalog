using Microsoft.EntityFrameworkCore;
using WebApiApp.Application.Exceptions;
using WebApiApp.Contracts;
using WebApiApp.Data;
using WebApiApp.Models;
using WebApiApp.Services;

namespace WebApiApp.Application.Categories;

public interface ICategoryApplicationService
{
    Task<IReadOnlyList<CategoryResponse>> GetCategoriesAsync(CancellationToken cancellationToken);

    Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken);

    Task<CategoryResponse?> UpdateCategoryAsync(int id, UpdateCategoryRequest request, CancellationToken cancellationToken);

    Task<bool> DeleteCategoryAsync(int id, CancellationToken cancellationToken);
}

public sealed class CategoryApplicationService(
    AppDbContext dbContext,
    IOperationLogService operationLogService) : ICategoryApplicationService
{
    public async Task<IReadOnlyList<CategoryResponse>> GetCategoriesAsync(CancellationToken cancellationToken) =>
        await dbContext.Categories
            .AsNoTracking()
            .OrderBy(category => category.CategoryId)
            .Select(category => new CategoryResponse(category.CategoryId, category.CategoryName))
            .ToListAsync(cancellationToken);

    public async Task<CategoryResponse> CreateCategoryAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedName = NormalizeCategoryName(request.CategoryName);

        var exists = await dbContext.Categories
            .AnyAsync(category => category.CategoryName == normalizedName, cancellationToken);

        if (exists)
        {
            throw new ConflictException("Категория с таким названием уже существует.");
        }

        var category = new Category { CategoryName = normalizedName };
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync(cancellationToken);

        await operationLogService.LogAsync(
            nameof(Category),
            category.CategoryId,
            "Create",
            new { category.CategoryName },
            httpContext: null,
            cancellationToken: cancellationToken);

        return new CategoryResponse(category.CategoryId, category.CategoryName);
    }

    public async Task<CategoryResponse?> UpdateCategoryAsync(
        int id,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedName = NormalizeCategoryName(request.CategoryName);

        var category = await dbContext.Categories
            .FirstOrDefaultAsync(item => item.CategoryId == id, cancellationToken);

        if (category is null)
        {
            return null;
        }

        var exists = await dbContext.Categories
            .AnyAsync(item => item.CategoryId != id && item.CategoryName == normalizedName, cancellationToken);

        if (exists)
        {
            throw new ConflictException("Категория с таким названием уже существует.");
        }

        category.CategoryName = normalizedName;
        await dbContext.SaveChangesAsync(cancellationToken);

        await operationLogService.LogAsync(
            nameof(Category),
            category.CategoryId,
            "Update",
            new { category.CategoryId, category.CategoryName },
            httpContext: null,
            cancellationToken: cancellationToken);

        return new CategoryResponse(category.CategoryId, category.CategoryName);
    }

    public async Task<bool> DeleteCategoryAsync(int id, CancellationToken cancellationToken)
    {
        var category = await dbContext.Categories
            .FirstOrDefaultAsync(item => item.CategoryId == id, cancellationToken);

        if (category is null)
        {
            return false;
        }

        var products = await dbContext.ProductCatalogs
            .Where(product => product.ProductCathegory == id)
            .ToListAsync(cancellationToken);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        dbContext.ProductCatalogs.RemoveRange(products);
        dbContext.Categories.Remove(category);

        foreach (var product in products)
        {
            await operationLogService.LogAsync(
                nameof(ProductCatalog),
                product.ProductId,
                "Delete",
                new { product.ProductId, product.ProductName, product.ProductCathegory, DeletedWithCategoryId = id },
                httpContext: null,
                cancellationToken);
        }

        await operationLogService.LogAsync(
            nameof(Category),
            category.CategoryId,
            "Delete",
            new { category.CategoryId, category.CategoryName, DeletedProductCount = products.Count },
            httpContext: null,
            cancellationToken: cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    private static string NormalizeCategoryName(string categoryName)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
        {
            throw new ArgumentException("Название категории обязательно.", nameof(categoryName));
        }

        return categoryName.Trim();
    }
}
