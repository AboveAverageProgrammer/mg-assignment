using ProductManagerApi.Models;

namespace ProductManagerApi.Repositories;

public class ProductRepository : IProductRepository
{
   private readonly ProductManagerApiContext _context;
   public ProductRepository(ProductManagerApiContext context)
   {
      _context = context;
   }
   public IEnumerable<ProductList> GetProductListAsync()
   {
      return _context.Products
         .Select(x => new ProductList(x.Id, x.Name, x.Available, x.Price))
         .AsEnumerable();
   }
}