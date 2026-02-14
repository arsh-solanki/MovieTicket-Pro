using CategoryMvcApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CategoryMvcApp.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Shows")]
    public class AdminShowsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminShowsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var shows = await _context.Shows
                .Include(s => s.Movie)
                .Include(s => s.Screen)
                .OrderByDescending(s => s.StartDateTime)
                .ToListAsync();
            return View("~/Views/Admin/Shows/Index.cshtml", shows);
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Movies = await _context.Movies.Where(m => m.IsActive).ToListAsync();
            ViewBag.Screens = await _context.Screens.Where(s => s.IsActive).ToListAsync();
            return View("~/Views/Admin/Shows/CreateEdit.cshtml", new Show());
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Show model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Movies = await _context.Movies.Where(m => m.IsActive).ToListAsync();
                ViewBag.Screens = await _context.Screens.Where(s => s.IsActive).ToListAsync();
                return View("~/Views/Admin/Shows/CreateEdit.cshtml", model);
            }

            model.IsActive = true;
            model.CreatedOn = DateTime.UtcNow;
            _context.Shows.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var show = await _context.Shows.FindAsync(id);
            if (show == null) return NotFound();
            ViewBag.Movies = await _context.Movies.Where(m => m.IsActive).ToListAsync();
            ViewBag.Screens = await _context.Screens.Where(s => s.IsActive).ToListAsync();
            return View("~/Views/Admin/Shows/CreateEdit.cshtml", show);
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Show model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid)
            {
                ViewBag.Movies = await _context.Movies.Where(m => m.IsActive).ToListAsync();
                ViewBag.Screens = await _context.Screens.Where(s => s.IsActive).ToListAsync();
                return View("~/Views/Admin/Shows/CreateEdit.cshtml", model);
            }

            var show = await _context.Shows.FindAsync(id);
            if (show == null) return NotFound();

            show.MovieId = model.MovieId;
            show.ScreenId = model.ScreenId;
            show.StartDateTime = model.StartDateTime;
            show.EndDateTime = model.EndDateTime;
            show.Price = model.Price;
            show.Status = model.Status;
            show.IsActive = model.IsActive;
            show.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var show = await _context.Shows.FindAsync(id);
            if (show == null) return NotFound();
            show.IsActive = false;
            show.ModifiedOn = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
