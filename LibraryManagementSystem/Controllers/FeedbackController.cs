using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Authorize]
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FeedbackController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==========================================
        // MEMBER - MY FEEDBACK
        // ==========================================

        [Authorize(Roles = "Member")]
        public async Task<IActionResult> MyFeedback()
        {
            var member = await GetCurrentMemberAsync();

            if (member == null)
                return NotFound("Member profile not found.");

            var feedbacks = await _context.Feedbacks
                .Include(f => f.Book)
                .Where(f => f.MemberID == member.MemberID)
                .OrderByDescending(f => f.DateSubmitted)
                .ToListAsync();

            return View(feedbacks);
        }

        // ==========================================
        // MEMBER - CREATE FEEDBACK
        // ==========================================

        [Authorize(Roles = "Member")]
        [HttpGet]
        public async Task<IActionResult> Create(int? bookId)
        {
            if (bookId == null)
                return NotFound();

            var member = await GetCurrentMemberAsync();

            if (member == null)
                return NotFound("Member profile not found.");

            var book = await _context.Books
                .FirstOrDefaultAsync(b => b.BookID == bookId);

            if (book == null)
                return NotFound();

            // Member must have borrowed this book
            var hasBorrowed = await _context.BorrowTransactions
                .AnyAsync(t =>
                    t.MemberID == member.MemberID &&
                    t.BookID == bookId &&
                    t.Status == "Returned");

            if (!hasBorrowed)
            {
                TempData["Error"] =
                    "You can only review a book after returning it.";

                return RedirectToAction(
                    "Details",
                    "Book",
                    new { id = bookId });
            }

            // One feedback per member per book
            var existingFeedback = await _context.Feedbacks
                .FirstOrDefaultAsync(f =>
                    f.MemberID == member.MemberID &&
                    f.BookID == bookId);

            if (existingFeedback != null)
            {
                TempData["Error"] =
                    "You have already submitted feedback for this book.";

                return RedirectToAction(
                    "Details",
                    "Book",
                    new { id = bookId });
            }

            ViewBag.Book = book;

            return View();
        }

        // ==========================================
        // MEMBER - CREATE FEEDBACK POST
        // ==========================================

        [Authorize(Roles = "Member")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int bookId,
            int rating,
            string? comment)
        {
            var member = await GetCurrentMemberAsync();

            if (member == null)
                return NotFound("Member profile not found.");

            var book = await _context.Books
                .FirstOrDefaultAsync(b => b.BookID == bookId);

            if (book == null)
                return NotFound();

            if (rating < 1 || rating > 5)
            {
                TempData["Error"] =
                    "Rating must be between 1 and 5.";

                return RedirectToAction(
                    "Details",
                    "Book",
                    new { id = bookId });
            }

            // Check borrowing history
            var hasBorrowed = await _context.BorrowTransactions
                .AnyAsync(t =>
                    t.MemberID == member.MemberID &&
                    t.BookID == bookId &&
                    t.Status == "Returned");

            if (!hasBorrowed)
            {
                TempData["Error"] =
                    "You can only review a book after returning it.";

                return RedirectToAction(
                    "Details",
                    "Book",
                    new { id = bookId });
            }

            // Check duplicate feedback
            var existingFeedback = await _context.Feedbacks
                .FirstOrDefaultAsync(f =>
                    f.MemberID == member.MemberID &&
                    f.BookID == bookId);

            if (existingFeedback != null)
            {
                TempData["Error"] =
                    "You have already reviewed this book.";

                return RedirectToAction(
                    "Details",
                    "Book",
                    new { id = bookId });
            }

            var feedback = new Feedback
            {
                MemberID = member.MemberID,
                BookID = bookId,
                Rating = rating,
                Comment = comment,
                DateSubmitted = DateTime.Now
            };

            _context.Feedbacks.Add(feedback);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Your feedback has been submitted successfully.";

            return RedirectToAction(
                "Details",
                "Book",
                new { id = bookId });
        }

        // ==========================================
        // LIBRARIAN - ALL FEEDBACK
        // ==========================================

        [Authorize(Roles = "Librarian")]
        public async Task<IActionResult> Index()
        {
            var feedbacks = await _context.Feedbacks
                .Include(f => f.Book)
                .Include(f => f.Member)
                .OrderByDescending(f => f.DateSubmitted)
                .ToListAsync();

            return View(feedbacks);
        }

        // ==========================================
        // LIBRARIAN - DELETE FEEDBACK
        // ==========================================

        [Authorize(Roles = "Librarian")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var feedback = await _context.Feedbacks
                .FirstOrDefaultAsync(f => f.FeedbackID == id);

            if (feedback == null)
                return NotFound();

            _context.Feedbacks.Remove(feedback);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Feedback deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // HELPER
        // ==========================================

        private async Task<Member?> GetCurrentMemberAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return null;

            return await _context.Members
                .FirstOrDefaultAsync(m =>
                    m.ApplicationUserId == user.Id);
        }
    }
}