using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace ProductManagerApi.Entities;

public class Product
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Required]
    public int Id { get; set; }
    [Required]
    [StringLength(50, MinimumLength = 5, ErrorMessage = "Name must be between 5 and 100 characters.")]
    public string Name { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    [Required]
    public decimal Price { get; set; }
    public bool Available { get; set; }
    [StringLength(150, ErrorMessage = "Description must be maximum 150 characters.")]
    public string? Description { get; set; }
    [Column(TypeName = "timestamp")]
    public DateTime DateCreated { get; set; } = new();
    [Timestamp] // For concurrency check
    public byte[] RowVersion { get; set; }
}