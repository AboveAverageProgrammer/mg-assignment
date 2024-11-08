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
            builder.Property(x => x.RowVersion)
                .HasColumnName("xmin") // Map to PostgreSQL's xmin column
                .HasColumnType("xid")
                .IsConcurrencyToken(); // Mark it as a concurrency token
            builder.Property(x => x.Available)
                .IsRequired();
        });
    }
}