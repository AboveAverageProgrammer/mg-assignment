using ProductManagerApi.Entities;
using ProductManagerApi.Models;

namespace ProductManagerApi.Repositories;

public interface IProductRepository
{
    IEnumerable<ProductList> GetProductListAsync();
}