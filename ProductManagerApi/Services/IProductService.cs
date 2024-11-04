using ProductManagerApi.Models;

namespace ProductManagerApi.Services;

public interface IProductService
{
    IEnumerable<ProductList> GetProductListAsync();
}