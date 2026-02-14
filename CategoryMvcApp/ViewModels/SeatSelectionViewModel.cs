using System.ComponentModel.DataAnnotations;

namespace CategoryMvcApp.ViewModels
{
    /// <summary>
    /// Core ViewModel for seat selection/booking page.
    /// Displays seat grid, booked seats, pricing, and movie/show information.
    /// Used on Booking/SelectSeats/{showId} page with jQuery/AJAX integration.
    /// </summary>
    public class SeatSelectionViewModel
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

        [Display(Name = "Seat Prices")]
        public List<SeatCategoryPriceViewModel> Prices { get; set; } = new List<SeatCategoryPriceViewModel>();

        [Display(Name = "Booked Seats")]
        public List<string> BookedSeats { get; set; } = new List<string>();

        [Display(Name = "Theatre Layout")]
        public string LayoutJson { get; set; } // JSON stored in Screen.LayoutJson (format: {"rows":10,"cols":15,"categories":{"A1-A10":"Premium","B1-B15":"Regular"}}

        [Display(Name = "Total Rows")]
        public int Rows { get; set; }

        [Display(Name = "Total Columns")]
        public int Columns { get; set; }

        /// <summary>
        /// Usage: In Booking/SelectSeats/{showId} GET action.
        /// 
        /// Example:
        /// var show = await _context.Shows
        ///     .Include(s => s.Movie)
        ///     .Include(s => s.Screen).ThenInclude(sc => sc.Theatre)
        ///     .Include(s => s.SeatPrices)
        ///     .Include(s => s.BookedSeats)
        ///     .FirstOrDefaultAsync(s => s.Id == showId);
        ///
        /// var vm = new SeatSelectionViewModel
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
        ///         .ToList(),
        ///     BookedSeats = show.BookedSeats.Where(bs => bs.IsActive)
        ///         .Select(bs => bs.SeatNumber)
        ///         .ToList(),
        ///     LayoutJson = show.Screen.LayoutJson
        /// };
        /// return View(vm);
        /// </summary>

        /// <summary>
        /// AJAX Integration:
        /// 1. Frontend jQuery code should:
        ///    - Render seat grid based on LayoutJson (parse rows/cols)
        ///    - Mark booked seats as disabled
        ///    - Allow user to click available seats to select them
        ///    - Show selected seats with their categories and prices
        ///    - Display running total amount
        ///
        /// 2. When user clicks "Reserve Seats" button, send AJAX POST:
        ///    POST /Booking/ReserveSeats
        ///    Content-Type: application/json
        ///    Body: { "showId": @Model.ShowId, "seatNumbers": ["A1", "A2", ...] }
        ///    
        /// 3. Receive ReserveSeatsResponse and handle success/error
        /// </summary>
    }
}
