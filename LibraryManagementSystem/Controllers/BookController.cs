using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    public class BookController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public BookController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // ==========================================
        // BOOK CATALOGUE
        // Accessible by Members and Librarians
        // ==========================================

        [Authorize]
        public async Task<IActionResult> Index(
            string? search,
            string? genre)
        {
            var query = _context.Books
                .Include(b => b.Library)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(b =>
                    b.Title.Contains(search) ||
                    b.Author.Contains(search) ||
                    b.ISBN.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(genre))
            {
                query = query.Where(b => b.Genre == genre);
            }

            ViewBag.Search = search;
            ViewBag.Genre = genre;

            ViewBag.Genres = await _context.Books
                .Select(b => b.Genre)
                .Distinct()
                .OrderBy(g => g)
                .ToListAsync();

            return View(await query
                .OrderBy(b => b.Title)
                .ToListAsync());
        }


        // ==========================================
        // BOOK DETAILS
        // ==========================================

        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var book = await _context.Books
                .Include(b => b.Library)
                .Include(b => b.Feedbacks)
                    .ThenInclude(f => f.Member)
                .FirstOrDefaultAsync(b => b.BookID == id);

            if (book == null)
                return NotFound();

            return View(book);
        }


        // ==========================================
        // CREATE BOOK
        // Librarian only
        // ==========================================

        [Authorize(Roles = "Librarian")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Libraries = await _context.Libraries
                .OrderBy(l => l.Name)
                .ToListAsync();

            return View();
        }


        [Authorize(Roles = "Librarian")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Book book,
            IFormFile? coverImage)
        {
            if (await _context.Books
                .AnyAsync(b => b.ISBN == book.ISBN))
            {
                ModelState.AddModelError(
                    "ISBN",
                    "A book with this ISBN already exists.");
            }

            if (ModelState.IsValid)
            {
                if (coverImage != null &&
                    coverImage.Length > 0)
                {
                    var imageUrl =
                        await SaveCoverImage(coverImage);

                    if (imageUrl == null)
                    {
                        ModelState.AddModelError(
                            "CoverImageUrl",
                            "Invalid cover image.");
                    }
                    else
                    {
                        book.CoverImageUrl = imageUrl;
                    }
                }

                if (ModelState.IsValid)
                {
                    book.DateAdded = DateTime.Now;

                    if (string.IsNullOrWhiteSpace(
                        book.AvailabilityStatus))
                    {
                        book.AvailabilityStatus = "Available";
                    }

                    _context.Books.Add(book);

                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
            }

            ViewBag.Libraries = await _context.Libraries
                .OrderBy(l => l.Name)
                .ToListAsync();

            return View(book);
        }


        // ==========================================
        // EDIT BOOK
        // Librarian only
        // ==========================================

        [Authorize(Roles = "Librarian")]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var book = await _context.Books.FindAsync(id);

            if (book == null)
                return NotFound();

            ViewBag.Libraries = await _context.Libraries
                .OrderBy(l => l.Name)
                .ToListAsync();

            return View(book);
        }


        [Authorize(Roles = "Librarian")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Book book,
            IFormFile? coverImage)
        {
            if (id != book.BookID)
                return NotFound();

            if (await _context.Books.AnyAsync(
                b => b.ISBN == book.ISBN &&
                     b.BookID != book.BookID))
            {
                ModelState.AddModelError(
                    "ISBN",
                    "A book with this ISBN already exists.");
            }

            if (ModelState.IsValid)
            {
                var existingBook =
                    await _context.Books.FindAsync(id);

                if (existingBook == null)
                    return NotFound();

                existingBook.Title = book.Title;
                existingBook.Author = book.Author;
                existingBook.Genre = book.Genre;
                existingBook.ISBN = book.ISBN;
                existingBook.Summary = book.Summary;
                existingBook.AvailabilityStatus =
                    book.AvailabilityStatus;
                existingBook.LibraryID = book.LibraryID;

                if (coverImage != null &&
                    coverImage.Length > 0)
                {
                    var imageUrl =
                        await SaveCoverImage(coverImage);

                    if (imageUrl == null)
                    {
                        ModelState.AddModelError(
                            "CoverImageUrl",
                            "Invalid cover image.");
                    }
                    else
                    {
                        existingBook.CoverImageUrl = imageUrl;
                    }
                }

                if (ModelState.IsValid)
                {
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
            }

            ViewBag.Libraries = await _context.Libraries
                .OrderBy(l => l.Name)
                .ToListAsync();

            return View(book);
        }


        // ==========================================
        // DELETE BOOK
        // Librarian only
        // ==========================================

        [Authorize(Roles = "Librarian")]
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var book = await _context.Books
                .Include(b => b.Library)
                .FirstOrDefaultAsync(b => b.BookID == id);

            if (book == null)
                return NotFound();

            return View(book);
        }


        [Authorize(Roles = "Librarian")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book != null)
            {
                _context.Books.Remove(book);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    TempData["Error"] =
                        "This book cannot be deleted because it has existing borrowing, reservation, or feedback records.";

                    return RedirectToAction(nameof(Index));
                }
            }

            return RedirectToAction(nameof(Index));
        }


        // ==========================================
        // SAVE COVER IMAGE
        // ==========================================

        private async Task<string?> SaveCoverImage(
            IFormFile image)
        {
            const long maxFileSize = 2 * 1024 * 1024;

            if (image.Length > maxFileSize)
                return null;

            var allowedExtensions = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            var extension =
                Path.GetExtension(image.FileName)
                    .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return null;

            var uploadsFolder = Path.Combine(
                _environment.WebRootPath,
                "images",
                "covers");

            Directory.CreateDirectory(uploadsFolder);

            var fileName =
                $"{Guid.NewGuid()}{extension}";

            var filePath =
                Path.Combine(uploadsFolder, fileName);

            await using var stream =
                new FileStream(filePath, FileMode.Create);

            await image.CopyToAsync(stream);

            return $"/images/covers/{fileName}";
        }
    }
}