using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using ProductManagerApi.Entities;
using ProductManagerApi.Models;
using ProductManagerApi.Services;

namespace Tests;

public class Task4AddProductTest : ProductServiceBase
{
    [Fact]
    public async void AddNewProduct_success()
    {
        // Arrange
        var product = new Product
        {
            Name = "Product 4",
            Price = 100,
            Available = true
        };
        await _productService.AddProductAsync(product);
        var products = _productService.GetProductList();
        Assert.Contains(products, p => p.Name == product.Name);
    }
    [Fact]
    public async void AddNewInvalidProduct_fail()
    {
        // Arrange
        var product = new Product
        {
            Price = 100
        };
        await Assert.ThrowsAsync<ValidationException>(() => _productService.AddProductAsync(product));
    }

    [Fact]
    public async void AddDuplicateNameProduct_fail()
    {
        var product = new Product
        {
            Name = "Product 3",
            Price = 100,
            Available = true
        };
        await Assert.ThrowsAsync<DbUpdateException>(() => _productService.AddProductAsync(product));
    }
    [Fact]
    public async void AddDuplicateIdProduct_fail()
    {
        var product = new Product
        {
            Id = 1,
            Name = "Product 3",
            Price = 100
        };
        await Assert.ThrowsAsync<DuplicateIdException>(() => _productService.AddProductAsync(product));
    }

    [Fact]
    public async void AddProductSimeltaneously_success()
    {
        var product1 = new Product
        {
            Name = "Product 5",
            Price = 100,
            Available = true,
        };
        var product2 = new Product
        {
            Name = "Product 6",
            Price = 100,
            Available = true
        };
        var _productService2 = CreateNewProducService();
        await Task.WhenAll(_productService.AddProductAsync(product1), _productService2.AddProductAsync(product2));
        var products = _productService.GetProductList();
        Assert.Contains(products, p => p.Name == product1.Name);
        Assert.Contains(products, p => p.Name == product2.Name);
    }

    [Fact]
    public async void AddDuplicateNameProductSimeltaneously_fail()
    {
        var product1 = new Product
        {
            Name = "Product 5",
            Price = 100,
            Available = true
        };
        var product2 = new Product
        {
            Name = "Product 5",
            Price = 200,
            Available = true
        };
        var _productService2 = CreateNewProducService();
        var addAll = Task.WhenAll(_productService.AddProductAsync(product1),
            _productService2.AddProductAsync(product2));
        await Assert.ThrowsAsync<DbUpdateException>(() => addAll);
        var products = _productService.GetProductList().ToList();
        Assert.Equal(4, products?.Count());
        Assert.Contains(products, p => p.Name == product1.Name);
        // Assert.Contains(products, p => p.Name == product2.Name);
    }
}