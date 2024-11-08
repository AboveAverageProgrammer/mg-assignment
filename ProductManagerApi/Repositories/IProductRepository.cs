using ProductManagerApi.Entities;
using ProductManagerApi.Models;

namespace ProductManagerApi.Repositories;

public interface IProductRepository
{
    IEnumerable<ProductList> GetProductListAsync();
    Task<Product?> GetProductByIdAsync(int id);
    Task AddProductAsync(Product product);
    Task UpdateProductAsync(Product product);
    Task<bool> CheckProductExistAsync(int id);
    Task DeleteProductAsync(Product product);
}