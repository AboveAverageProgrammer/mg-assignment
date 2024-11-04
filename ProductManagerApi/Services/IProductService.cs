using ProductManagerApi.Entities;
using ProductManagerApi.Models;

namespace ProductManagerApi.Services;

public interface IProductService
{
    IEnumerable<ProductList> GetProductList();
    Task<Product> GetProductByIdAsync(int id);
    Task AddProductAsync(Product product);
}