using System.ComponentModel.DataAnnotations;
using ProductManagerApi.Entities;
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
    public IEnumerable<ProductList> GetProductList()
    {
        return _productRepository.GetProductListAsync();
    }

    public async Task<Product> GetProductByIdAsync(int id)
    {
        var product = await _productRepository.GetProductByIdAsync(id);
        if (product == null)
        {
            throw new EntityNotFoundException(nameof(Product),id);
        }
        return product;
    }

    public Task AddProductAsync(Product product)
    {
        Validator.ValidateObject(product, new ValidationContext(product), true);
        return _productRepository.AddProductAsync(product);
    }
    public Task UpdateProductAsync(Product product)
    {
        Validator.ValidateObject(product, new ValidationContext(product), true);
        return _productRepository.UpdateProductAsync(product);
    }
}