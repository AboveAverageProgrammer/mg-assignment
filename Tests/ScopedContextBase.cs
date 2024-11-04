using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProductManagerApi;
using ProductManagerApi.Entities;

namespace Tests;

public class ScopedContextBase : IDisposable
{
    internal readonly ProductManagerApiContext _context;
    internal readonly ProductManagerApiContext _context2;
    internal DbContextOptions<ProductManagerApiContext> _contextOptions;

    protected ScopedContextBase()
    {
        var (localContextOptions, localConnString) = CreateNewContextOptions(GetType().Name);
        _contextOptions = localContextOptions;
        Console.WriteLine(localConnString);
        _context = new (localContextOptions);
        _context2 = new (localContextOptions); 
        _context.Database.EnsureCreated();
        InsertTestData();
    }
    public void Dispose()
    {
        _context.Database.EnsureDeleted();
    }
    private void InsertTestData()
    {
        _context.Products.Add(new Product { Name = "Product 1", Price = 10 ,Available = true});
        _context.Products.Add(new Product { Name = "Product 2", Price = 20 ,Available = false});
        _context.Products.Add(new Product { Name = "Product 3", Price = 30 ,Available = false});
        _context.SaveChanges();
    }
    internal static (DbContextOptions<ProductManagerApiContext> contextOptions, string connString) CreateNewContextOptions(
        string testName)
    {
        var dbSuffix = $"{testName}_{Guid.NewGuid()}";
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkNpgsql()
            .BuildServiceProvider();
         
        var password = "root";
        var database = $"ProductManagerApi_Test_{dbSuffix}";
        var connectionString = $"Host=localhost; Port=5432; database={database}; Username=admin; Password={password}; Trust Server Certificate = true;Include Error Detail = true;";
        var builder = new DbContextOptionsBuilder<ProductManagerApiContext>();
        builder.UseNpgsql(connectionString)
            .UseInternalServiceProvider(serviceProvider);
        return (builder.Options, connectionString);
    }
}