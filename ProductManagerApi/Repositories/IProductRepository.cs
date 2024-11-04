using ProductManagerApi.Entities;
using ProductManagerApi.Models;

namespace ProductManagerApi.Repositories;

public interface IProductRepository
{
    IEnumerable<ProductList> GetProductListAsync();
    Task<Product?> GetProductByIdAsync(int id);
    Task AddProductAsync(Product product);
}