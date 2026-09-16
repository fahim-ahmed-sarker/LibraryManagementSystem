using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Member")]
    public class BorrowTransactionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly BorrowingService _borrowingService;

        public BorrowTransactionController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            BorrowingService borrowingService)
        {
            _context = context;
            _userManager = userManager;
            _borrowingService = borrowingService;
        }

        // ==========================================
        // MY CURRENT BORROWINGS
        // ==========================================

        public async Task<IActionResult> Index()
        {
            var member = await GetCurrentMemberAsync();

            if (member == null)
                return NotFound("Member profile not found.");

            await _borrowingService.UpdateOverdueStatusesAsync();

            var transactions =
                await _context.BorrowTransactions
                    .Include(t => t.Book)
                    .Where(t =>
                        t.MemberID == member.MemberID &&
                        (t.Status == "Borrowed" ||
                         t.Status == "Overdue"))
                    .OrderBy(t => t.DueDate)
                    .ToListAsync();

            return View(transactions);
        }


        // ==========================================
        // BORROW
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Borrow(int bookId)
        {
            var member = await GetCurrentMemberAsync();

            if (member == null)
                return NotFound("Member profile not found.");

            var result =
                await _borrowingService
                    .BorrowBookAsync(
                        member.MemberID,
                        bookId);

            TempData[result.Success ? "Success" : "Error"] =
                result.Message;

            return RedirectToAction(
                "Details",
                "Book",
                new { id = bookId });
        }


        // ==========================================
        // RETURN
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(
            int transactionId)
        {
            var member = await GetCurrentMemberAsync();

            if (member == null)
                return NotFound("Member profile not found.");

            var result =
                await _borrowingService
                    .ReturnBookAsync(
                        member.MemberID,
                        transactionId);

            TempData[result.Success ? "Success" : "Error"] =
                result.Message;

            return RedirectToAction(nameof(Index));
        }


        // ==========================================
        // RENEW
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Renew(
            int transactionId)
        {
            var member = await GetCurrentMemberAsync();

            if (member == null)
                return NotFound("Member profile not found.");

            var result =
                await _borrowingService
                    .RenewBookAsync(
                        member.MemberID,
                        transactionId);

            TempData[result.Success ? "Success" : "Error"] =
                result.Message;

            return RedirectToAction(nameof(Index));
        }


        // ==========================================
        // BORROWING HISTORY
        // ==========================================

        public async Task<IActionResult> History()
        {
            var member = await GetCurrentMemberAsync();

            if (member == null)
                return NotFound("Member profile not found.");

            var transactions =
                await _context.BorrowTransactions
                    .Include(t => t.Book)
                    .Include(t => t.Fine)
                    .Where(t =>
                        t.MemberID == member.MemberID)
                    .OrderByDescending(t => t.BorrowDate)
                    .ToListAsync();

            return View(transactions);
        }


        // ==========================================
        // CURRENT MEMBER
        // ==========================================

        private async Task<Member?> GetCurrentMemberAsync()
        {
            var user = await _userManager
                .GetUserAsync(User);

            if (user == null)
                return null;

            return await _context.Members
                .FirstOrDefaultAsync(m =>
                    m.ApplicationUserId == user.Id);
        }

        // ==========================================
        // LIBRARIAN - TRANSACTION MANAGEMENT
        // ==========================================

        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> Manage(string? status)
        {
            var query = _context.BorrowTransactions
                .Include(t => t.Member)
                .Include(t => t.Book)
                .Include(t => t.Fine)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(t => t.Status == status);
            }

            var transactions = await query
                .OrderByDescending(t => t.BorrowDate)
                .ToListAsync();

            ViewBag.Status = status;

            return View(transactions);
        }
    }
}