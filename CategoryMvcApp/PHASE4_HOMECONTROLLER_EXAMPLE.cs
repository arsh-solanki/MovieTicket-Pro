using CategoryMvcApp.Models;
using CategoryMvcApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CategoryMvcApp.Controllers
{
    /// <summary>
    /// Optional example: Updated HomeController using Phase 4 ViewModels.
    /// This is a reference implementation showing how to integrate ViewModels.
    /// 
    /// INTEGRATION STEPS:
    /// 1. Replace existing Views/Home/Index.cshtml with strongly-typed version
    /// 2. Update HomeController.Index() to return List<MovieCardViewModel>
    /// 3. Add 'using CategoryMvcApp.ViewModels;' at top
    /// 4. Map Movie entities to MovieCardViewModel in the Index action
    /// 5. Test: dotnet run, navigate to http://localhost:5000, verify movie grid
    /// </summary>
    public class HomeControllerExample : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Dependency injection: ILogger for logging, ApplicationDbContext for data access.
        /// </summary>
        public HomeControllerExample(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Home/Index action: Display all active movies in grid format.
        /// 
        /// BEFORE (Returns Entity Model directly - not recommended):
        ///     public async Task<IActionResult> Index()
        ///     {
        ///         var movies = await _context.Movies.Where(m => m.IsActive).ToListAsync();
        ///         return View(movies);  // Exposes full entity to view
        ///     }
        ///
        /// AFTER (Returns ViewModels - best practice):
        ///     public async Task<IActionResult> Index()
        ///     {
        ///         var movies = await _context.Movies.Where(m => m.IsActive).ToListAsync();
        ///         var viewModels = movies.Select(m => new MovieCardViewModel { ... }).ToList();
        ///         return View(viewModels);  // Only exposes view-needed data
        ///     }
        ///
        /// Benefits of using ViewModels:
        /// • Separation of concerns (Entity ≠ View)
        /// • Security (don't expose sensitive properties to client)
        /// • Flexibility (ViewModel can have calculated/derived fields)
        /// • Testability (easier to mock ViewModels in unit tests)
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                // Step 1: Fetch active movies from database
                var movies = await _context.Movies
                    .Where(m => m.IsActive)
                    .OrderByDescending(m => m.ReleaseDate)  // Most recent first
                    .ToListAsync();

                // Step 2: Map Movie entities to MovieCardViewModel (transformation)
                var movieCardViewModels = movies.Select(movie => new MovieCardViewModel
                {
                    Id = movie.Id,
                    Title = movie.Title,
                    PosterUrl = movie.PosterUrl,
                    ReleaseDate = movie.ReleaseDate,
                    Description = movie.Description?.Length > 100
                        ? movie.Description.Substring(0, 100) + "..."  // Truncate long descriptions
                        : movie.Description ?? "No description available",  // Null coalescing
                    IsActive = movie.IsActive
                }).ToList();

                _logger.LogInformation($"Loaded {movieCardViewModels.Count} active movies for home page");

                // Step 3: Return View with strongly-typed ViewModel
                return View(movieCardViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading movies on home page: {ex.Message}");
                // In production, return error view instead of throwing
                return View(new List<MovieCardViewModel>());
            }
        }

        /// <summary>
        /// Example: GetMovieDetails action (if you want to implement).
        /// Demonstrates MovieDetailViewModel with embedded ShowListItemViewModel.
        /// 
        /// Route: GET /Movies/Details/{id}
        /// Returns: MovieDetailViewModel with list of active shows
        /// </summary>
        public async Task<IActionResult> GetMovieDetails(int id)
        {
            // Fetch movie with related shows and pricing
            var movie = await _context.Movies
                .Include(m => m.Shows)
                    .ThenInclude(s => s.Screen)
                        .ThenInclude(sc => sc.Theatre)
                .Include(m => m.Shows)
                    .ThenInclude(s => s.SeatPrices)
                .FirstOrDefaultAsync(m => m.Id == id && m.IsActive);

            if (movie == null)
            {
                _logger.LogWarning($"Movie with ID {id} not found");
                return NotFound();
            }

            // Map to MovieDetailViewModel with embedded shows
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

                // Nested: List of active shows mapped to ShowListItemViewModel
                ShowTimes = movie.Shows
                    .Where(s => s.IsActive && s.StartDateTime > DateTime.UtcNow)  // Only future shows
                    .Select(show => new ShowListItemViewModel
                    {
                        ShowId = show.Id,
                        StartDateTime = show.StartDateTime,
                        EndDateTime = show.EndDateTime,
                        ScreenName = show.Screen?.Name ?? "Unknown Screen",
                        TheatreName = show.Screen?.Theatre?.Name ?? "Unknown Theatre",
                        City = show.Screen?.Theatre?.City ?? "Unknown City",

                        // Calculate price range for display
                        PriceRange = show.SeatPrices.Any()
                            ? $"₹{show.SeatPrices.Min(p => p.Price)} – ₹{show.SeatPrices.Max(p => p.Price)}"
                            : "Price TBD",

                        IsActive = show.IsActive
                    })
                    .OrderBy(s => s.StartDateTime)  // Sort by earliest time first
                    .ToList()
            };

            return View(viewModel);
        }

        /// <summary>
        /// Example: SearchMovies action demonstrating ViewModel filtering.
        /// Route: GET /Movies/Search?query=Batman
        /// </summary>
        public async Task<IActionResult> SearchMovies(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return View(new List<MovieCardViewModel>());
            }

            // Search by title or description
            var movies = await _context.Movies
                .Where(m => m.IsActive &&
                    (m.Title.Contains(query) || m.Description.Contains(query)))
                .ToListAsync();

            // Map to ViewModels
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

            ViewBag.Query = query;
            ViewBag.ResultCount = viewModels.Count;

            return View("Index", viewModels);  // Reuse Index.cshtml view
        }

        /// <summary>
        /// Privacy page (unchanged from template).
        /// </summary>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Error page (unchanged from template).
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    /// <summary>
    /// STRONGLY-TYPED RAZOR VIEW EXAMPLE (Views/Home/Index.cshtml)
    /// 
    /// Replace the existing Index.cshtml with this:
    /// 
    /// @model List<CategoryMvcApp.ViewModels.MovieCardViewModel>
    /// 
    /// @{
    ///     ViewData["Title"] = "Home";
    /// }
    /// 
    /// <div class="container-fluid mt-5">
    ///     <h1 class="mb-5">🎬 Now Showing</h1>
    /// 
    ///     @if (Model == null || Model.Count == 0)
    ///     {
    ///         <div class="alert alert-info">
    ///             <p>No movies currently available.</p>
    ///         </div>
    ///     }
    ///     else
    ///     {
    ///         <div class="row g-4">
    ///             @foreach (var movie in Model)
    ///             {
    ///                 <div class="col-lg-3 col-md-4 col-sm-6">
    ///                     <div class="card h-100 shadow-sm hover-shadow">
    ///                         <img src="@movie.PosterUrl" 
    ///                              class="card-img-top movie-poster" 
    ///                              alt="@movie.Title"
    ///                              style="height: 300px; object-fit: cover;" />
    ///                         <div class="card-body d-flex flex-column">
    ///                             <h5 class="card-title">
    ///                                 @Html.DisplayFor(m => movie.Title)
    ///                             </h5>
    ///                             <p class="card-text text-muted small">
    ///                                 Release: @movie.ReleaseDate.ToShortDateString()
    ///                             </p>
    ///                             <p class="card-text">
    ///                                 @Html.DisplayFor(m => movie.Description)
    ///                             </p>
    ///                             <a href="/Movies/Details/@movie.Id" 
    ///                                class="btn btn-primary mt-auto">
    ///                                 View Showtimes →
    ///                             </a>
    ///                         </div>
    ///                     </div>
    ///                 </div>
    ///             }
    ///         </div>
    ///     }
    /// </div>
    /// 
    /// <style>
    ///     .movie-poster {
    ///         cursor: pointer;
    ///         transition: transform 0.2s;
    ///     }
    ///     .movie-poster:hover {
    ///         transform: scale(1.05);
    ///     }
    ///     .hover-shadow {
    ///         transition: box-shadow 0.2s;
    ///     }
    ///     .hover-shadow:hover {
    ///         box-shadow: 0 8px 16px rgba(0, 0, 0, 0.15) !important;
    ///     }
    /// </style>
    /// </summary>
}
