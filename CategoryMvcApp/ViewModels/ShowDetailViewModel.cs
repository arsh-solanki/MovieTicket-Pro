using System.ComponentModel.DataAnnotations;

namespace CategoryMvcApp.ViewModels
{
    /// <summary>
    /// ViewModel for displaying show details before user selects seats.
    /// Used on show info page (intermediate step before seat selection).
    /// </summary>
    public class ShowDetailViewModel
    {
        [Display(Name = "Show ID")]
        public int ShowId { get; set; }

        [Display(Name = "Movie Title")]
        [Required]
        [StringLength(200)]
        public string MovieTitle { get; set; }

        [Display(Name = "Poster")]
        [Url]
        public string PosterUrl { get; set; }

        [Display(Name = "Theatre")]
        [Required]
        [StringLength(200)]
        public string TheatreName { get; set; }

        [Display(Name = "Screen")]
        [Required]
        [StringLength(100)]
        public string ScreenName { get; set; }

        [Display(Name = "Show Time")]
        [DataType(DataType.DateTime)]
        public DateTime StartDateTime { get; set; }

        [Display(Name = "Duration (minutes)")]
        public int DurationMinutes { get; set; }

        [Display(Name = "Available Seat Prices")]
        public List<SeatCategoryPriceViewModel> Prices { get; set; } = new List<SeatCategoryPriceViewModel>();

        /// <summary>
        /// Usage: In Shows/Details/{id} action to show show details and prices.
        /// Navigate to SeatSelectionViewModel after user confirms.
        /// 
        /// Example:
        /// var show = await _context.Shows
        ///     .Include(s => s.Movie)
        ///     .Include(s => s.Screen).ThenInclude(sc => sc.Theatre)
        ///     .Include(s => s.SeatPrices)
        ///     .FirstOrDefaultAsync(s => s.Id == id);
        /// var vm = new ShowDetailViewModel
        /// {
        ///     ShowId = show.Id,
        ///     MovieTitle = show.Movie.Title,
        ///     PosterUrl = show.Movie.PosterUrl,
        ///     TheatreName = show.Screen.Theatre.Name,
        ///     ScreenName = show.Screen.Name,
        ///     StartDateTime = show.StartDateTime,
        ///     DurationMinutes = show.Movie.DurationMinutes,
        ///     Prices = show.SeatPrices
        ///         .Select(p => new SeatCategoryPriceViewModel { Category = p.Category, Price = p.Price })
        ///         .ToList()
        /// };
        /// </summary>
    }
}
