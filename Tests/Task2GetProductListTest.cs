using System.Reflection.Metadata;
using ProductManagerApi.Repositories;
using ProductManagerApi.Services;

namespace Tests;

public class Task2GetProductListTest : ProductServiceBase 
{
    [Fact]
    public void Test_success()
    {
        var result = _productService.GetProductListAsync().ToList();
        Assert.Equal(3, result.Count()); // 3 products are inserted in the database
        Assert.Equal("Product 1", result[0].Name);
        Assert.Equal("Product 2", result[1].Name);
        Assert.Equal("Product 3", result[2].Name);
        Assert.False(result[0].Available);
        Assert.False(result[1].Available);
        Assert.False(result[2].Available);
    }
}