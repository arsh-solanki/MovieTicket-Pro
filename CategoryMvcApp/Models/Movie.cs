using System.ComponentModel.DataAnnotations;

namespace CategoryMvcApp.Models
{
    /// <summary>
    /// Movie – Represents a movie in the system.
    /// 
    /// Data Dictionary:
    /// - Id (Primary Key): Unique auto-increment identifier
    /// - Title: Movie name (required, max 200 chars)
    /// - Description: Plot synopsis (optional)
    /// - DurationMinutes: Total runtime (non-negative integer)
    /// - ReleaseDate: Release date (date only)
    /// - PosterUrl: Image URL for movie poster (optional)
    /// - TrailerUrl: YouTube/video URL (optional)
    /// - IsActive: Flag for active/inactive status
    /// - CreatedAt: Timestamp of creation
    /// - Shows: Navigation to all shows for this movie
    /// 
    /// Admin can create, update (title, duration, release date, status).
    /// Used to populate UI dropdowns, listing, search.
    /// </summary>
    public class Movie
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Duration must be greater than 0.")]
        public int DurationMinutes { get; set; }

        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; }

        [Url(ErrorMessage = "Invalid URL format.")]
        public string? PosterUrl { get; set; }

        [Url(ErrorMessage = "Invalid URL format.")]
        public string? TrailerUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public virtual ICollection<Show> Shows { get; set; } = new List<Show>();
    }
}
