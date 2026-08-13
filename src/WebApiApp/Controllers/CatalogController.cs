using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebApiApp.Application.Catalog;
using WebApiApp.Auth;
using WebApiApp.Contracts;

namespace WebApiApp.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CatalogController(ICatalogApplicationService catalogService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CatalogItemResponse>>> GetCatalog(
        [FromQuery] string? search,
        [FromQuery] string? categoryName,
        [FromQuery] decimal? minCost,
        [FromQuery] decimal? maxCost,
        CancellationToken cancellationToken)
    {
        var items = await catalogService.GetCatalogAsync(search, categoryName, minCost, maxCost, cancellationToken);
        return Ok(items);
    }

    [HttpGet("export/pdf")]
    [SwaggerOperation(
        Summary = "Экспорт каталога товаров в PDF",
        Description = "Возвращает PDF-файл с товарами из product_catalog, отфильтрованными по поиску, категории и диапазону цены.")]
    [Produces("application/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportCatalogPdf(
        [FromQuery(Name = "search")] string? search,
        [FromQuery(Name = "categoryName")] string? categoryName,
        [FromQuery(Name = "minCost")] decimal? minCost,
        [FromQuery(Name = "maxCost")] decimal? maxCost,
        CancellationToken cancellationToken)
    {
        var pdfBytes = await catalogService.ExportPdfAsync(search, categoryName, minCost, maxCost, cancellationToken);
        return File(pdfBytes, "application/pdf", $"catalog_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf");
    }

    [HttpPost]
    [Authorize(Roles = $"{AppRoles.User},{AppRoles.AdvancedUser}")]
    public async Task<ActionResult<CatalogItemResponse>> CreateProduct(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await catalogService.CreateProductAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetCatalog), new { id = product.ProductId }, product);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{AppRoles.User},{AppRoles.AdvancedUser}")]
    public async Task<ActionResult<CatalogItemResponse>> ChangeProduct(
        int id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await catalogService.UpdateProductAsync(id, request, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = AppRoles.AdvancedUser)]
    public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken)
    {
        var deleted = await catalogService.DeleteProductAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
