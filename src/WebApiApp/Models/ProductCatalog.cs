namespace WebApiApp.Models;

public class ProductCatalog
{
    public int ProductId { get; set; }

    public string? ProductName { get; set; }

    public int? ProductCathegory { get; set; }

    public Category? Category { get; set; }

    public string? ProductDescription { get; set; }

    public decimal ProductCost { get; set; }

    public string? ProductNote { get; set; }

    public string? ProductNoteSpecial { get; set; }
}
