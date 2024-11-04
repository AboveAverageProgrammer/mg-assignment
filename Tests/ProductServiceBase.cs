using ProductManagerApi;
using ProductManagerApi.Repositories;
using ProductManagerApi.Services;

namespace Tests;

public class ProductServiceBase : ScopedContextBase
{
    protected readonly IProductService _productService;
    public ProductServiceBase()
    {
        _productService = CreateNewProducService(); 
    }
    internal IProductService CreateNewProducService()
    {
        return new ProductService(new ProductRepository(new(_contextOptions)));
    }
}