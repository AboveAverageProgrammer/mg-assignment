using Microsoft.EntityFrameworkCore;
using ProductManagerApi.Entities;

namespace ProductManagerApi;

public class ProductManagerApiContext : DbContext
{
    public ProductManagerApiContext(){}
    public ProductManagerApiContext(DbContextOptions<ProductManagerApiContext> options) : base(options)
    {
    }

    public ProductManagerApiContext(string connString)
    {
        var options = new DbContextOptionsBuilder<ProductManagerApiContext>()
            .UseNpgsql(connString)
            .Options;
        Database.EnsureCreated();
    }

    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(builder =>
        {
            builder.HasIndex(x => x.Name).IsUnique();
            builder.Property(x => x.Name).IsRequired();
        });

        modelBuilder.Entity<Product>()
            .Property(x => x.Xmin)
            .IsRowVersion();
    }
}