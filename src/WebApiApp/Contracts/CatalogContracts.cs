namespace WebApiApp.Contracts;

public record CatalogItemResponse(
    int ProductId,
    string? ProductName,
    string? ProductCathegory,
    string? ProductDescription,
    decimal ProductCost,
    string? ProductNote,
    string? ProductNoteSpecial);

public record CreateProductRequest(
    string? ProductName,
    int? ProductCathegory,
    string? ProductDescription,
    decimal ProductCost,
    string? ProductNote,
    string? ProductNoteSpecial);

public record UpdateProductRequest(
    string? ProductName,
    int? ProductCathegory,
    string? ProductDescription,
    decimal ProductCost,
    string? ProductNote,
    string? ProductNoteSpecial);
