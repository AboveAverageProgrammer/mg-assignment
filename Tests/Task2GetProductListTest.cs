using ProductManagerApi.Repositories;
using ProductManagerApi.Services;

namespace Tests;

public class Task2GetProductListTest : ProductServiceBase 
{
    [Fact]
    public void Test_success()
    {
        var result = _productService.GetProductListAsync();
        Assert.Equal(3, result.Count()); // 3 products are inserted in the database
    }
}