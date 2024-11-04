using Microsoft.EntityFrameworkCore;
using ProductManagerApi.Entities;

namespace ProductManagerApi;

public class ProductManagerApiContext : DbContext
{
    public ProductManagerApiContext(){}
    public ProductManagerApiContext(DbContextOptions<ProductManagerApiContext> options) : base(options)
    {
    }
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(builder =>
        {
            builder.HasIndex(x => x.Name).IsUnique();
            builder.Property(x => x.RowVersion)
                .IsRowVersion();
        });
    }
}