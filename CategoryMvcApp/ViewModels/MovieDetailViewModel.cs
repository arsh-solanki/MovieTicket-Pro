using System.ComponentModel.DataAnnotations;

namespace CategoryMvcApp.ViewModels
{
    /// <summary>
    /// ViewModel for displaying detailed movie information.
    /// Used on Movies/Details/{id} page with list of available showtimes.
    /// </summary>
    public class MovieDetailViewModel
    {
        [Display(Name = "Movie ID")]
        public int Id { get; set; }

        [Display(Name = "Movie Title")]
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Duration")]
        public int DurationMinutes { get; set; }

        [Display(Name = "Release Date")]
        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; }

        [Display(Name = "Poster Image")]
        [Url]
        public string PosterUrl { get; set; }

        [Display(Name = "Trailer URL")]
        [Url]
        public string TrailerUrl { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        /// <summary>
        /// List of available showtimes for this movie.
        /// Populated from Show entities filtered by this MovieId.
        /// </summary>
        [Display(Name = "Available Showtimes")]
        public List<ShowListItemViewModel> ShowTimes { get; set; } = new List<ShowListItemViewModel>();

        /// <summary>
        /// Usage: In Movies/Details/{id} action:
        /// var movie = await _context.Movies
        ///     .Include(m => m.Shows).ThenInclude(s => s.SeatPrices)
        ///     .FirstOrDefaultAsync(m => m.Id == id);
        /// var vm = new MovieDetailViewModel
        /// {
        ///     Id = movie.Id,
        ///     Title = movie.Title,
        ///     Description = movie.Description,
        ///     DurationMinutes = movie.DurationMinutes,
        ///     ReleaseDate = movie.ReleaseDate,
        ///     PosterUrl = movie.PosterUrl,
        ///     TrailerUrl = movie.TrailerUrl,
        ///     IsActive = movie.IsActive,
        ///     ShowTimes = movie.Shows.Where(s => s.IsActive)
        ///         .Select(s => new ShowListItemViewModel { ... }).ToList()
        /// };
        /// </summary>
    }
}
