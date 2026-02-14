using CategoryMvcApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CategoryMvcApp.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Movies")]
    public class AdminMoviesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminMoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies.OrderByDescending(m => m.ReleaseDate).ToListAsync();
            return View("~/Views/Admin/Movies/Index.cshtml", movies);
        }

        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View("~/Views/Admin/Movies/CreateEdit.cshtml", new Movie());
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Movie model)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Admin/Movies/CreateEdit.cshtml", model);

            model.IsActive = true;
            model.CreatedOn = DateTime.UtcNow;
            _context.Movies.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();
            return View("~/Views/Admin/Movies/CreateEdit.cshtml", movie);
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Movie model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View("~/Views/Admin/Movies/CreateEdit.cshtml", model);

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();

            movie.Title = model.Title;
            movie.Description = model.Description;
            movie.Language = model.Language;
            movie.Duration = model.Duration;
            movie.Genre = model.Genre;
            movie.ReleaseDate = model.ReleaseDate;
            movie.PosterUrl = model.PosterUrl;
            movie.IsActive = model.IsActive;
            movie.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();
            movie.IsActive = false; // soft delete
            movie.ModifiedOn = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
