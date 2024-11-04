using ProductManagerApi.Repositories;
using ProductManagerApi.Services;

namespace Tests;

public class ProductServiceBase : ScopedContextBase
{
    protected readonly IProductService _productService;
    public ProductServiceBase()
    {
        _productService = new ProductService(new ProductRepository(_context));
    }
}