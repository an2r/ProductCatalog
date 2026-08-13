namespace WebApiApp.Contracts;

public record CategoryResponse(int CategoryId, string CategoryName);

public record CreateCategoryRequest(string CategoryName);

public record UpdateCategoryRequest(string CategoryName);
