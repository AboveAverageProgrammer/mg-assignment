using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductManagerApi.Entities;

[Table("products")]
public class Product
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [StringLength(50, MinimumLength = 5, ErrorMessage = "Name must be between 5 and 100 characters.")]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("price", TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; }

    [Column("available")]
    public bool Available { get; set; }

    [StringLength(150, ErrorMessage = "Description must be maximum 150 characters.")]
    [Column("description")]
    public string? Description { get; set; }

    [Column("date_created", TypeName = "timestamp")]
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    public uint Xmin{ get; private set; } 
}