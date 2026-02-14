using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CategoryMvcApp.Models;
using CategoryMvcApp.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CategoryMvcApp.Controllers;

/// <summary>
/// HomeController: Main landing page with movie grid.
/// Demonstrates Phase 4 ViewModel integration with MovieCardViewModel.
/// </summary>
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// GET: /
    /// Home page: Display active movies in grid format using MovieCardViewModel.
    /// 
    /// @model List<MovieCardViewModel>
    /// Returns grid of movie cards with poster, title, release date, description.
    /// Links to Movies/Details/{id} for showtime listings.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        try
        {
            // Load all active movies, ordered by release date (most recent first)
            var movies = await _context.Movies
                .Where(m => m.IsActive)
                .OrderByDescending(m => m.ReleaseDate)
                .ToListAsync();

            // Map Movie entities to MovieCardViewModel
            var movieCardViewModels = movies.Select(movie => new MovieCardViewModel
            {
                Id = movie.Id,
                Title = movie.Title,
                PosterUrl = movie.PosterUrl,
                ReleaseDate = movie.ReleaseDate,
                Description = movie.Description?.Length > 100
                    ? movie.Description.Substring(0, 100) + "..."
                    : movie.Description ?? "No description available",
                IsActive = movie.IsActive
            }).ToList();

            _logger.LogInformation($"Home/Index: Loaded {movieCardViewModels.Count} active movies");
            return View(movieCardViewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in Home/Index: {ex.Message}");
            return View(new List<MovieCardViewModel>());
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
