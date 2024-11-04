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
    private async Task CheckProductExist(int id)
    {
        if (!await _productRepository.CheckProductExistAsync(id))
        {
            throw new EntityNotFoundException(nameof(Product),id);
        }
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

    public async Task AddProductAsync(Product product)
    {
        var alreadyExists = await _productRepository.CheckProductExistAsync(product.Id);
        if (alreadyExists)
        {
            throw new DuplicateIdException(nameof(product), product.Id);
        }
        Validator.ValidateObject(product, new ValidationContext(product), true);
        await _productRepository.AddProductAsync(product);
    }
    public async Task UpdateProductAsync(Product product)
    {
        if (product.RowVersion == 0)
        {
            var foundProduct = await _productRepository.GetProductByIdAsync(product.Id);
            if (foundProduct == null)
            {
                throw new EntityNotFoundException(nameof(Product), product.Id);
            }
            foundProduct.Price = product.Price;
            foundProduct.Available = product.Available;
            foundProduct.Name = product.Name;
            foundProduct.Description = product.Description;
            product = foundProduct;
        }
        Validator.ValidateObject(product, new ValidationContext(product), true);
        await _productRepository.UpdateProductAsync(product);
    }
}