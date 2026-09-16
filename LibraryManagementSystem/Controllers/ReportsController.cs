using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Librarian")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // REPORT DASHBOARD
        // ==========================================

        public async Task<IActionResult> Index()
        {
            var totalBooks = await _context.Books.CountAsync();

            var totalMembers = await _context.Members.CountAsync();

            var totalBorrowings =
                await _context.BorrowTransactions.CountAsync();

            var activeBorrowings =
                await _context.BorrowTransactions
                    .CountAsync(t =>
                        t.Status == "Borrowed" ||
                        t.Status == "Overdue");

            var overdueBooks =
                await _context.BorrowTransactions
                    .CountAsync(t =>
                        t.Status == "Overdue" ||
                        (t.Status == "Borrowed" &&
                         t.ReturnDate == null &&
                         t.DueDate < DateTime.Now));

            var unpaidFines =
                await _context.Fines
                    .Where(f => !f.IsPaid)
                    .SumAsync(f => (decimal?)f.Amount) ?? 0;

            ViewBag.TotalBooks = totalBooks;
            ViewBag.TotalMembers = totalMembers;
            ViewBag.TotalBorrowings = totalBorrowings;
            ViewBag.ActiveBorrowings = activeBorrowings;
            ViewBag.OverdueBooks = overdueBooks;
            ViewBag.UnpaidFines = unpaidFines;

            return View();
        }


        // ==========================================
        // BORROWING TRENDS
        // ==========================================

        public async Task<IActionResult> BorrowingTrends()
        {
            var data = await _context.BorrowTransactions
                .GroupBy(t => new
                {
                    t.BorrowDate.Year,
                    t.BorrowDate.Month
                })
                .Select(g => new BorrowingTrendViewModel
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    BorrowCount = g.Count()
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            return View(data);
        }


        // ==========================================
        // OVERDUE BOOKS
        // ==========================================

        public async Task<IActionResult> OverdueBooks()
        {
            var data = await _context.BorrowTransactions
                .Include(t => t.Member)
                .Include(t => t.Book)
                .Include(t => t.Fine)
                .Where(t =>
                    t.Status == "Overdue" ||
                    (t.Status == "Borrowed" &&
                     t.DueDate < DateTime.Now &&
                     t.ReturnDate == null))
                .OrderBy(t => t.DueDate)
                .ToListAsync();

            return View(data);
        }


        // ==========================================
        // MOST ACTIVE MEMBERS
        // ==========================================

        public async Task<IActionResult> MostActiveMembers()
        {
            var data = await _context.Members
                .Select(m => new MostActiveMemberViewModel
                {
                    MemberID = m.MemberID,
                    MemberName = m.Name,
                    Email = m.Email,
                    BorrowCount = m.Transactions.Count
                })
                .OrderByDescending(x => x.BorrowCount)
                .ToListAsync();

            return View(data);
        }


        // ==========================================
        // MOST POPULAR BOOKS
        // ==========================================

        public async Task<IActionResult> MostPopularBooks()
        {
            var data = await _context.Books
                .Select(b => new MostPopularBookViewModel
                {
                    BookID = b.BookID,
                    Title = b.Title,
                    Author = b.Author,
                    Genre = b.Genre,
                    BorrowCount = b.Transactions.Count,
                    AverageRating = b.Feedbacks.Any()
                        ? b.Feedbacks.Average(f => f.Rating)
                        : 0
                })
                .OrderByDescending(x => x.BorrowCount)
                .ThenByDescending(x => x.AverageRating)
                .ToListAsync();

            return View(data);
        }
    }


    // ==========================================
    // REPORT VIEW MODELS
    // ==========================================

    public class BorrowingTrendViewModel
    {
        public int Year { get; set; }

        public int Month { get; set; }

        public int BorrowCount { get; set; }

        public string MonthName =>
            new DateTime(Year, Month, 1)
                .ToString("MMMM yyyy");
    }


    public class MostActiveMemberViewModel
    {
        public int MemberID { get; set; }

        public string MemberName { get; set; } =
            string.Empty;

        public string Email { get; set; } =
            string.Empty;

        public int BorrowCount { get; set; }
    }


    public class MostPopularBookViewModel
    {
        public int BookID { get; set; }

        public string Title { get; set; } =
            string.Empty;

        public string Author { get; set; } =
            string.Empty;

        public string Genre { get; set; } =
            string.Empty;

        public int BorrowCount { get; set; }

        public double AverageRating { get; set; }
    }
}