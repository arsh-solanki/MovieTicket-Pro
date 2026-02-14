using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CategoryMvcApp.Models
{
    /// <summary>
    /// Screen – Represents a cinema auditorium within a theatre.
    /// 
    /// Data Dictionary:
    /// - Id (Primary Key): Unique auto-increment identifier
    /// - TheatreId (Foreign Key): Reference to parent theatre
    /// - Name: Screen name, e.g. "Screen 1", "IMAX" (max 100 chars)
    /// - TotalSeats: Total seating capacity (non-negative)
    /// - LayoutJson: JSON string storing seat layout configuration.
    ///   Example: {"rows": 10, "cols": 15, "categories": {"A1-A15": "Silver", "B1-B15": "Gold"}}
    /// - IsActive: Operational status
    /// - CreatedAt: Creation timestamp
    /// - Theatre: Navigation to parent theatre
    /// - Shows: Navigation to shows on this screen
    /// 
    /// LayoutJson enables dynamic seat grid rendering and price categorization.
    /// Unique index on (TheatreId, Name) prevents duplicate screen names per theatre.
    /// </summary>
    public class Screen
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Theatre")]
        public int TheatreId { get; set; }

        [Required(ErrorMessage = "Screen name is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Total seats must be greater than 0.")]
        public int TotalSeats { get; set; }

        /// <summary>
        /// JSON-formatted seat layout. Example:
        /// {"rows": 10, "cols": 15, "categories": {"A": "Silver", "B": "Gold", "C": "Platinum"}}
        /// </summary>
        [StringLength(5000, ErrorMessage = "Layout configuration is too large.")]
        public string? LayoutJson { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("TheatreId")]
        public virtual Theatre? Theatre { get; set; }

        public virtual ICollection<Show> Shows { get; set; } = new List<Show>();
    }
}
