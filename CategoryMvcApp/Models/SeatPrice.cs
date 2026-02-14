using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CategoryMvcApp.Models
{
    /// <summary>
    /// SeatPrice – Defines price for a seat category in a specific show.
    /// 
    /// Data Dictionary:
    /// - Id (Primary Key): Unique auto-increment identifier
    /// - ShowId (Foreign Key): Reference to the show
    /// - Category: Seat category name, e.g. "Silver", "Gold", "Platinum" (max 50 chars)
    /// - Price: Cost per seat in this category (non-negative decimal, 2 decimals)
    /// - CreatedAt: Creation timestamp
    /// - Show: Navigation to the show
    /// 
    /// Example: Show #5 may have Silver=300, Gold=500, Platinum=800.
    /// Prices are show-specific to allow dynamic pricing.
    /// </summary>
    public class SeatPrice
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Show")]
        public int ShowId { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Category must be between 1 and 50 characters.")]
        public string Category { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Price { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        [ForeignKey("ShowId")]
        public virtual Show? Show { get; set; }
    }
}
