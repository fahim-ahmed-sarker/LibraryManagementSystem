using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // ==========================================
            // NEW ARRIVALS
            // ==========================================

            var newArrivals = await _context.Books
                .Include(b => b.Library)
                .OrderByDescending(b => b.DateAdded)
                .Take(6)
                .ToListAsync();


            // ==========================================
            // MOST BORROWED BOOKS
            // ==========================================

            var mostBorrowed = await _context.Books
                .Include(b => b.Library)
                .OrderByDescending(b =>
                    b.Transactions.Count)
                .Take(6)
                .ToListAsync();


            // ==========================================
            // CURRENTLY AVAILABLE BOOKS
            // ==========================================

            var availableBooks = await _context.Books
                .Include(b => b.Library)
                .Where(b =>
                    b.AvailabilityStatus == "Available")
                .OrderBy(b => b.Title)
                .Take(6)
                .ToListAsync();


            // ==========================================
            // RECOMMENDED BOOKS
            // ==========================================
            //
            // For now, recommendations are based on
            // popular books. Later we can make this
            // personalized using member history.
            // ==========================================

            var recommendedBooks = await _context.Books
                .Include(b => b.Library)
                .Include(b => b.Feedbacks)
                .OrderByDescending(b =>
                    b.Feedbacks.Any()
                        ? b.Feedbacks.Average(f => f.Rating)
                        : 0)
                .ThenByDescending(b =>
                    b.Transactions.Count)
                .Take(6)
                .ToListAsync();

            ViewBag.TotalTitles = await _context.Books.CountAsync();
            ViewBag.AvailableTitleCount = await _context.Books
                .CountAsync(b => b.AvailabilityStatus == "Available");
            ViewBag.GenreCount = await _context.Books
                .Select(b => b.Genre)
                .Distinct()
                .CountAsync();


            // ==========================================
            // SEND DATA TO VIEW
            // ==========================================

            ViewBag.NewArrivals = newArrivals;
            ViewBag.MostBorrowed = mostBorrowed;
            ViewBag.AvailableBooks = availableBooks;
            ViewBag.RecommendedBooks = recommendedBooks;

            return View();
        }
    }
}