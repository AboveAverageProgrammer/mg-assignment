using ProductManagerApi.Models;
using ProductManagerApi.Repositories;

namespace ProductManagerApi.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }
    public IEnumerable<ProductList> GetProductListAsync()
    {
        return _productRepository.GetProductListAsync();
    }
}