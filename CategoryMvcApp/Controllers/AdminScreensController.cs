using CategoryMvcApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CategoryMvcApp.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Screens")]
    public class AdminScreensController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminScreensController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var screens = await _context.Screens
                .Include(s => s.Theatre)
                .OrderBy(s => s.Name)
                .ToListAsync();
            return View("~/Views/Admin/Screens/Index.cshtml", screens);
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Theatres = await _context.Theatres.Where(t => t.IsActive).ToListAsync();
            return View("~/Views/Admin/Screens/CreateEdit.cshtml", new Screen());
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Screen model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Theatres = await _context.Theatres.Where(t => t.IsActive).ToListAsync();
                return View("~/Views/Admin/Screens/CreateEdit.cshtml", model);
            }

            model.IsActive = true;
            model.CreatedOn = DateTime.UtcNow;
            _context.Screens.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var screen = await _context.Screens.FindAsync(id);
            if (screen == null) return NotFound();
            ViewBag.Theatres = await _context.Theatres.Where(t => t.IsActive).ToListAsync();
            return View("~/Views/Admin/Screens/CreateEdit.cshtml", screen);
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Screen model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid)
            {
                ViewBag.Theatres = await _context.Theatres.Where(t => t.IsActive).ToListAsync();
                return View("~/Views/Admin/Screens/CreateEdit.cshtml", model);
            }

            var screen = await _context.Screens.FindAsync(id);
            if (screen == null) return NotFound();

            screen.Name = model.Name;
            screen.SeatLayoutJson = model.SeatLayoutJson;
            screen.TheatreId = model.TheatreId;
            screen.IsActive = model.IsActive;
            screen.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var screen = await _context.Screens.FindAsync(id);
            if (screen == null) return NotFound();
            screen.IsActive = false;
            screen.ModifiedOn = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
