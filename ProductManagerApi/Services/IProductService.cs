using ProductManagerApi.Entities;
using ProductManagerApi.Models;

namespace ProductManagerApi.Services;

public interface IProductService
{
    IEnumerable<ProductList> GetProductListAsync();
    Task<Product> GetProductByIdAsync(int id);
}