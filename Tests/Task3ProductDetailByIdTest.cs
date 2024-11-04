using ProductManagerApi.Models;

namespace Tests;

public class Task3ProductDetailByIdTest : ProductServiceBase
{
   [Fact]
   public async void GetProductByIdTest_success()
   {
      var product1 = await _productService.GetProductByIdAsync(1);
      Assert.Equal("Product 1", product1.Name);
   }
   [Fact]
   public async void GetProductByIdTest_fail()
   {
      await Assert.ThrowsAsync<EntityNotFoundException>(() => _productService.GetProductByIdAsync(4));
   }
}