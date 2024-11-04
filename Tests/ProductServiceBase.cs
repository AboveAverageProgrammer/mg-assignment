using ProductManagerApi.Repositories;
using ProductManagerApi.Services;

namespace Tests;

public class ProductServiceBase : ScopedContextBase
{
    protected readonly IProductService _productService;
    protected readonly IProductService _productService2;
    public ProductServiceBase()
    {
        _productService = new ProductService(new ProductRepository(_context));
        _productService2 = new ProductService(new ProductRepository(_context2));
    }
}