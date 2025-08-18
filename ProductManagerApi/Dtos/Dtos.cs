namespace ProductManagerApi.Dtos;

public record ProductListItem(int Id, string Name, bool Available, decimal Price);
public record ProductDetail(int Id, string Name, bool Available, decimal Price, string? Description, DateTime DateCreated, uint Version);
public record CreateProduct(string Name, decimal Price, bool Available, string? Description);
public record UpdateProduct(string Name, decimal Price, bool Available, string? Description);
