using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = "Member")]
    public class ReservationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReservationController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==========================================
        // MY RESERVATIONS
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var member = await GetCurrentMemberAsync();

            if (member == null)
                return NotFound("Member profile not found.");

            var reservations = await _context.Reservations
                .Include(r => r.Book)
                .Where(r => r.MemberID == member.MemberID)
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();

            return View(reservations);
        }


        // ==========================================
        // CREATE RESERVATION
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Create(int? bookId)
        {
            if (bookId == null)
                return NotFound();

            var book = await _context.Books
                .FirstOrDefaultAsync(b => b.BookID == bookId);

            if (book == null)
                return NotFound();

            if (book.AvailabilityStatus == "Available")
            {
                TempData["Error"] =
                    "This book is currently available. You can borrow it instead.";

                return RedirectToAction(
                    "Details",
                    "Book",
                    new { id = bookId });
            }

            var member = await GetCurrentMemberAsync();

            if (member == null)
                return NotFound("Member profile not found.");

            var alreadyReserved = await _context.Reservations
                .AnyAsync(r =>
                    r.MemberID == member.MemberID &&
                    r.BookID == bookId &&
                    r.Status == "Pending");

            if (alreadyReserved)
            {
                TempData["Error"] =
                    "You have already reserved this book.";

                return RedirectToAction(
                    "Details",
                    "Book",
                    new { id = bookId });
            }

            ViewBag.Book = book;

            return View();
        }


        // ==========================================
        // CREATE RESERVATION POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int bookId)
        {
            var member = await GetCurrentMemberAsync();

            if (member == null)
                return NotFound("Member profile not found.");

            var book = await _context.Books
                .FirstOrDefaultAsync(b => b.BookID == bookId);

            if (book == null)
                return NotFound();

            if (book.AvailabilityStatus == "Available")
            {
                TempData["Error"] =
                    "This book is currently available. Please borrow it instead.";

                return RedirectToAction(
                    "Details",
                    "Book",
                    new { id = bookId });
            }

            var alreadyReserved = await _context.Reservations
                .AnyAsync(r =>
                    r.MemberID == member.MemberID &&
                    r.BookID == bookId &&
                    r.Status == "Pending");

            if (alreadyReserved)
            {
                TempData["Error"] =
                    "You have already reserved this book.";

                return RedirectToAction(nameof(Index));
            }

            var reservation = new Reservation
            {
                MemberID = member.MemberID,
                BookID = bookId,
                ReservationDate = DateTime.Now,
                Status = "Pending"
            };

            _context.Reservations.Add(reservation);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Book reserved successfully.";

            return RedirectToAction(nameof(Index));
        }


        // ==========================================
        // CANCEL RESERVATION
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(
            int reservationId)
        {
            var member = await GetCurrentMemberAsync();

            if (member == null)
                return NotFound("Member profile not found.");

            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r =>
                    r.ReservationID == reservationId &&
                    r.MemberID == member.MemberID);

            if (reservation == null)
                return NotFound();

            if (reservation.Status != "Pending")
            {
                TempData["Error"] =
                    "Only pending reservations can be cancelled.";

                return RedirectToAction(nameof(Index));
            }

            reservation.Status = "Cancelled";

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Reservation cancelled successfully.";

            return RedirectToAction(nameof(Index));
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
    }
}