using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using ProductManagerApi.Models;

namespace Tests;

public class Task6DeleteProductTest : ProductServiceBase
{
   [Fact]
   public async void DeleteProduct_success()
   {
      await _productService.DeleteProductAsync(2);
      var products = _productService.GetProductList().ToList();
      Assert.Equal(2,products.Count);
   }

   [Fact]
   public async void DeleteNonExistentProduct_fail()
   {
      var task = _productService.DeleteProductAsync(4);
      await Assert.ThrowsAsync<EntityNotFoundException>(() => task);
   }

   [Fact]
   public async void DeleteProductSimultaneously_fail()
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
            // Save changes and trigger a concurrency check
            await producService.DeleteProductAsync(productId);
         }
         catch (DbUpdateConcurrencyException ex)
         {
            // Capture concurrency exceptions for logging or further processing
            exceptions.Add(ex);
         }
         catch (Exception e)
         {            
            exceptions.Add(e);
         }
      });
      Assert.NotEmpty(exceptions); 
      Assert.Equal(3,exceptions.Count);    
   }
}