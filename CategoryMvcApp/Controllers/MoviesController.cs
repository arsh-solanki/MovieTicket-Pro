using CategoryMvcApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CategoryMvcApp.Controllers
{
    /// <summary>
    /// MoviesController: Handles movie listing and detail pages.
    /// Shows available showtimes for each movie using nested ViewModels.
    /// </summary>
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MoviesController> _logger;

        public MoviesController(ApplicationDbContext context, ILogger<MoviesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// GET: /Movies
        /// List all active movies with MovieCardViewModel.
        /// Similar to Home/Index but can have additional filters.
        /// 
        /// @model List<MovieCardViewModel>
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var movies = await _context.Movies
                    .Where(m => m.IsActive)
                    .OrderByDescending(m => m.ReleaseDate)
                    .ToListAsync();

                var viewModels = movies.Select(m => new MovieCardViewModel
                {
                    Id = m.Id,
                    Title = m.Title,
                    PosterUrl = m.PosterUrl,
                    ReleaseDate = m.ReleaseDate,
                    Description = m.Description?.Length > 100
                        ? m.Description.Substring(0, 100) + "..."
                        : m.Description,
                    IsActive = m.IsActive
                }).ToList();

                return View(viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Movies/Index: {ex.Message}");
                return View(new List<MovieCardViewModel>());
            }
        }

        /// <summary>
        /// GET: /Movies/Details/{id}
        /// Show movie details with available showtimes using MovieDetailViewModel.
        /// Includes nested ShowListItemViewModel for each active showtime.
        /// 
        /// @model MovieDetailViewModel
        /// Shows: Full movie info, poster, trailer, duration, release date
        /// Embedded: List of available showtimes with prices and theatre info
        /// User can click showtime to go to seat selection
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                // Load movie with shows and pricing
                var movie = await _context.Movies
                    .Include(m => m.Shows)
                        .ThenInclude(s => s.Screen)
                            .ThenInclude(sc => sc.Theatre)
                    .Include(m => m.Shows)
                        .ThenInclude(s => s.SeatPrices)
                    .FirstOrDefaultAsync(m => m.Id == id && m.IsActive);

                if (movie == null)
                {
                    _logger.LogWarning($"Movie {id} not found or inactive");
                    return NotFound("Movie not found");
                }

                // Map to MovieDetailViewModel with embedded ShowListItemViewModel list
                var viewModel = new MovieDetailViewModel
                {
                    Id = movie.Id,
                    Title = movie.Title,
                    Description = movie.Description,
                    DurationMinutes = movie.DurationMinutes,
                    ReleaseDate = movie.ReleaseDate,
                    PosterUrl = movie.PosterUrl,
                    TrailerUrl = movie.TrailerUrl,
                    IsActive = movie.IsActive,

                    // Nested ViewModels: Only show active, future shows
                    ShowTimes = movie.Shows
                        .Where(s => s.IsActive && s.StartDateTime > DateTime.UtcNow)
                        .OrderBy(s => s.StartDateTime)
                        .Select(s => new ShowListItemViewModel
                        {
                            ShowId = s.Id,
                            StartDateTime = s.StartDateTime,
                            EndDateTime = s.EndDateTime,
                            ScreenName = s.Screen?.Name ?? "Unknown Screen",
                            TheatreName = s.Screen?.Theatre?.Name ?? "Unknown Theatre",
                            City = s.Screen?.Theatre?.City ?? "Unknown City",

                            // Calculate price range
                            PriceRange = s.SeatPrices.Any()
                                ? $"₹{s.SeatPrices.Min(p => p.Price)} – ₹{s.SeatPrices.Max(p => p.Price)}"
                                : "Price TBD",

                            IsActive = s.IsActive
                        })
                        .ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Movies/Details: {ex.Message}");
                return StatusCode(500, "An error occurred while loading movie details");
            }
        }

        /// <summary>
        /// GET: /Movies/ShowDetails/{id}
        /// (Optional) Show details before seat selection.
        /// Intermediate page showing show info + prices before navigating to seat selection.
        /// 
        /// @model ShowDetailViewModel
        /// </summary>
        public async Task<IActionResult> ShowDetails(int id)
        {
            try
            {
                var show = await _context.Shows
                    .Include(s => s.Movie)
                    .Include(s => s.Screen)
                        .ThenInclude(sc => sc.Theatre)
                    .Include(s => s.SeatPrices)
                    .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

                if (show == null)
                {
                    return NotFound("Show not found");
                }

                var viewModel = new ShowDetailViewModel
                {
                    ShowId = show.Id,
                    MovieTitle = show.Movie.Title,
                    PosterUrl = show.Movie.PosterUrl,
                    TheatreName = show.Screen.Theatre.Name,
                    ScreenName = show.Screen.Name,
                    StartDateTime = show.StartDateTime,
                    DurationMinutes = show.Movie.DurationMinutes,
                    Prices = show.SeatPrices
                        .Select(p => new SeatCategoryPriceViewModel
                        {
                            Category = p.Category,
                            Price = p.Price
                        })
                        .ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Movies/ShowDetails: {ex.Message}");
                return StatusCode(500, "An error occurred");
            }
        }
    }
}
