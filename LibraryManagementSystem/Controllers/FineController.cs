using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Authorize]
    public class FineController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FineController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==========================================
        // MEMBER - MY FINES
        // ==========================================

        [Authorize(Roles = "Member")]
        public async Task<IActionResult> MyFines()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Unauthorized();

            var member = await _context.Members
                .FirstOrDefaultAsync(m =>
                    m.ApplicationUserId == user.Id);

            if (member == null)
                return NotFound("Member profile not found.");

            var fines = await _context.Fines
                .Include(f => f.Transaction)
                    .ThenInclude(t => t!.Book)
                .Include(f => f.Transaction)
                    .ThenInclude(t => t!.Member)
                .Where(f =>
                    f.Transaction != null &&
                    f.Transaction.MemberID == member.MemberID)
                .OrderByDescending(f => f.Transaction!.DueDate)
                .ToListAsync();

            return View(fines);
        }

        // ==========================================
        // LIBRARIAN - ALL FINES
        // ==========================================

        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> Index()
        {
            var fines = await _context.Fines
                .Include(f => f.Transaction)
                    .ThenInclude(t => t!.Book)
                .Include(f => f.Transaction)
                    .ThenInclude(t => t!.Member)
                .OrderByDescending(f => f.IsPaid)
                .ThenByDescending(f => f.Amount)
                .ToListAsync();

            return View(fines);
        }

        // ==========================================
        // LIBRARIAN - MARK PAID
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> MarkPaid(int id)
        {
            var fine = await _context.Fines
                .FirstOrDefaultAsync(f => f.FineID == id);

            if (fine == null)
                return NotFound();

            if (fine.IsPaid)
            {
                TempData["Error"] = "This fine has already been paid.";
                return RedirectToAction(nameof(Index));
            }

            fine.IsPaid = true;
            fine.PaidDate = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Fine marked as paid successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}