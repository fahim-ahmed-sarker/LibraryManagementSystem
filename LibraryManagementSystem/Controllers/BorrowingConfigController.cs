using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Librarian")]
    public class BorrowingConfigController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BorrowingConfigController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: BorrowingConfig
        public async Task<IActionResult> Index()
        {
            var configs = await _context.BorrowingConfigs
                .Include(c => c.Library)
                .ToListAsync();

            return View(configs);
        }

        // GET: BorrowingConfig/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Libraries = await _context.Libraries
                .OrderBy(l => l.Name)
                .ToListAsync();

            return View();
        }

        // POST: BorrowingConfig/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            BorrowingConfig config)
        {
            if (await _context.BorrowingConfigs
                .AnyAsync(c => c.LibraryID == config.LibraryID))
            {
                ModelState.AddModelError(
                    "LibraryID",
                    "This library already has borrowing settings.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Libraries = await _context.Libraries
                    .OrderBy(l => l.Name)
                    .ToListAsync();

                return View(config);
            }

            _context.BorrowingConfigs.Add(config);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: BorrowingConfig/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var config = await _context.BorrowingConfigs
                .Include(c => c.Library)
                .FirstOrDefaultAsync(c => c.ConfigID == id);

            if (config == null)
                return NotFound();

            return View(config);
        }

        // POST: BorrowingConfig/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            BorrowingConfig config)
        {
            if (id != config.ConfigID)
                return NotFound();

            if (!ModelState.IsValid)
            {
                return View(config);
            }

            var existingConfig =
                await _context.BorrowingConfigs
                    .FindAsync(id);

            if (existingConfig == null)
                return NotFound();

            existingConfig.LoanDurationDays =
                config.LoanDurationDays;

            existingConfig.RenewalLimit =
                config.RenewalLimit;

            existingConfig.OverduePenaltyPerDay =
                config.OverduePenaltyPerDay;

            existingConfig.MaxBorrowableItems =
                config.MaxBorrowableItems;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}