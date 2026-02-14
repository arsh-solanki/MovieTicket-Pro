using System.ComponentModel.DataAnnotations;

namespace CategoryMvcApp.ViewModels
{
    /// <summary>
    /// ViewModel for displaying movies in grid/list format.
    /// Used on Home/Index and Movies/List pages.
    /// </summary>
    public class MovieCardViewModel
    {
        [Display(Name = "Movie ID")]
        public int Id { get; set; }

        [Display(Name = "Movie Title")]
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Display(Name = "Poster")]
        [Url]
        public string PosterUrl { get; set; }

        [Display(Name = "Release Date")]
        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; }

        [Display(Name = "Description")]
        [StringLength(500)]
        public string Description { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        /// <summary>
        /// Usage: In Home/Index action, select all active movies and project to MovieCardViewModel.
        /// 
        /// Example:
        /// var movies = await _context.Movies.Where(m => m.IsActive).ToListAsync();
        /// var vm = movies.Select(m => new MovieCardViewModel 
        /// { 
        ///     Id = m.Id, 
        ///     Title = m.Title, 
        ///     PosterUrl = m.PosterUrl, 
        ///     ReleaseDate = m.ReleaseDate,
        ///     Description = m.Description.Length > 100 ? m.Description.Substring(0, 100) + "..." : m.Description,
        ///     IsActive = m.IsActive
        /// }).ToList();
        /// </summary>
    }
}
