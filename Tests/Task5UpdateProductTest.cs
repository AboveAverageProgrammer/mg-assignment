using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using ProductManagerApi.Entities;

namespace Tests;

public class Task5UpdateProductTest : ProductServiceBase
{
    [Fact]
    public async void UpdateProduct_success()
    {
        var product = await _productService.GetProductByIdAsync(1);
        product.Name = "Product 4";
        await _productService.UpdateProductAsync(product);
        var updatedProduct = await _productService.GetProductByIdAsync(1);
        Assert.Equal("Product 4", updatedProduct.Name);
    }

    [Fact]
    public async void UpdateInvalidProduct_fail()
    {
        var product = await _productService.GetProductByIdAsync(1);
        product.Name = "";
        await Assert.ThrowsAsync<ValidationException>(() => _productService.UpdateProductAsync(product));
    }

    [Fact]
    public async void UpdateSameProductSimeltaneously_fail()
    {
        var productId = 1;
        var updates = new[] { 100, 200, 300, 400 }; // Different price increments to apply in parallel
        var exceptions = new ConcurrentBag<Exception>();
        await Parallel.ForEachAsync(updates, async (increment, _) =>
        {
            try
            {
                // Create a new DbContext instance for each iteration
                var producService = CreateNewProducService();

                var product = await producService.GetProductByIdAsync(productId);
                product.Price = increment; // Apply a different increment for each task
                    // Save changes and trigger a concurrency check
                await producService.UpdateProductAsync(product);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Capture concurrency exceptions for logging or further processing
                exceptions.Add(ex);
            }
        });
        Assert.NotEmpty(exceptions); 
        Assert.Equal(3,exceptions.Count);
    }
}