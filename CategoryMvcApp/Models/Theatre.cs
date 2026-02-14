using System.ComponentModel.DataAnnotations;

namespace CategoryMvcApp.Models
{
    /// <summary>
    /// Theatre – Represents a cinema theatre/venue.
    /// 
    /// Data Dictionary:
    /// - Id (Primary Key): Unique auto-increment identifier
    /// - Name: Theatre name (required, max 150 chars)
    /// - City: Location city (required, max 100 chars)
    /// - Address: Full street address (max 500 chars)
    /// - IsActive: Operational status flag
    /// - CreatedAt: Creation timestamp
    /// - Screens: Navigation to screens in this theatre
    /// 
    /// Admin manages theatre details, availability.
    /// Used to group screens and shows geographically.
    /// </summary>
    public class Theatre
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Theatre name is required.")]
        [StringLength(150, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 150 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "City must be between 1 and 100 characters.")]
        public string City { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters.")]
        public string? Address { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public virtual ICollection<Screen> Screens { get; set; } = new List<Screen>();
    }
}
