using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApiApp.Application.Categories;
using WebApiApp.Auth;
using WebApiApp.Contracts;

namespace WebApiApp.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CategoriesController(ICategoryApplicationService categoryService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = AppRoles.AdvancedUser)]
    public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetCategories(CancellationToken cancellationToken)
    {
        var categories = await categoryService.GetCategoriesAsync(cancellationToken);
        return Ok(categories);
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.AdvancedUser)]
    public async Task<ActionResult<CategoryResponse>> CreateCategory(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var category = await categoryService.CreateCategoryAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetCategories), new { id = category.CategoryId }, category);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = AppRoles.AdvancedUser)]
    public async Task<ActionResult<CategoryResponse>> UpdateCategory(
        int id,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var category = await categoryService.UpdateCategoryAsync(id, request, cancellationToken);
        return category is null ? NotFound() : Ok(category);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = $"{AppRoles.AdvancedUser},{AppRoles.Administrator}")]
    public async Task<IActionResult> DeleteCategory(int id, CancellationToken cancellationToken)
    {
        var deleted = await categoryService.DeleteCategoryAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
