namespace WebApiApp.Models;

public class Category
{
    public int CategoryId { get; set; }

    public required string CategoryName { get; set; }

    public ICollection<ProductCatalog> Products { get; set; } = [];
}
