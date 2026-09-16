using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Services
{
    public class BorrowingService
    {
        private readonly ApplicationDbContext _context;

        public BorrowingService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // BORROW BOOK
        // ==========================================

        public async Task<(bool Success, string Message)>
            BorrowBookAsync(int memberId, int bookId)
        {
            var member = await _context.Members
                .FindAsync(memberId);

            if (member == null)
                return (false, "Member not found.");

            var book = await _context.Books
                .Include(b => b.Library)
                .FirstOrDefaultAsync(b => b.BookID == bookId);

            if (book == null)
                return (false, "Book not found.");

            if (book.AvailabilityStatus != "Available")
                return (false, "This book is not currently available.");

            var config = await _context.BorrowingConfigs
                .FirstOrDefaultAsync(c =>
                    c.LibraryID == book.LibraryID);

            if (config == null)
            {
                return (
                    false,
                    "Borrowing configuration has not been set for this library.");
            }

            var activeBorrowCount =
                await _context.BorrowTransactions
                    .CountAsync(t =>
                        t.MemberID == memberId &&
                        (t.Status == "Borrowed" ||
                         t.Status == "Overdue"));

            if (activeBorrowCount >= config.MaxBorrowableItems)
            {
                return (
                    false,
                    $"You can borrow a maximum of {config.MaxBorrowableItems} items.");
            }

            var alreadyBorrowed =
                await _context.BorrowTransactions
                    .AnyAsync(t =>
                        t.MemberID == memberId &&
                        t.BookID == bookId &&
                        (t.Status == "Borrowed" ||
                         t.Status == "Overdue"));

            if (alreadyBorrowed)
                return (false, "You already have this book.");

            var now = DateTime.Now;

            var transaction = new BorrowTransaction
            {
                MemberID = memberId,
                BookID = bookId,
                BorrowDate = now,
                DueDate = now.AddDays(config.LoanDurationDays),
                Status = "Borrowed",
                RenewalCount = 0
            };

            book.AvailabilityStatus = "Borrowed";

            _context.BorrowTransactions.Add(transaction);

            await _context.SaveChangesAsync();

            return (
                true,
                $"Book borrowed successfully. Due date: {transaction.DueDate:d}");
        }


        // ==========================================
        // RETURN BOOK
        // ==========================================

        public async Task<(bool Success, string Message)>
            ReturnBookAsync(
                int memberId,
                int transactionId)
        {
            var transaction =
                await _context.BorrowTransactions
                    .Include(t => t.Book)
                    .FirstOrDefaultAsync(t =>
                        t.TransactionID == transactionId &&
                        t.MemberID == memberId);

            if (transaction == null)
                return (false, "Borrowing transaction not found.");

            if (transaction.Status != "Borrowed" &&
                transaction.Status != "Overdue")
            {
                return (false, "This transaction cannot be returned.");
            }

            return await CompleteReturnAsync(transaction);
        }

        public async Task<(bool Success, string Message)>
            ReturnBookForLibrarianAsync(int transactionId)
        {
            var transaction = await _context.BorrowTransactions
                .Include(t => t.Book)
                .FirstOrDefaultAsync(t =>
                    t.TransactionID == transactionId);

            if (transaction == null)
                return (false, "Borrowing transaction not found.");

            if (transaction.Status != "Borrowed" &&
                transaction.Status != "Overdue")
            {
                return (false, "This transaction cannot be returned.");
            }

            return await CompleteReturnAsync(transaction);
        }

        private async Task<(bool Success, string Message)>
            CompleteReturnAsync(BorrowTransaction transaction)
        {
            transaction.ReturnDate = DateTime.Now;

            var overdueDays = Math.Max(
                0,
                (transaction.ReturnDate.Value.Date -
                 transaction.DueDate.Date).Days);

            if (overdueDays > 0)
            {
                var config = await _context.BorrowingConfigs
                    .FirstOrDefaultAsync(c =>
                        c.LibraryID == transaction.Book!.LibraryID);

                if (config != null)
                {
                    var fineAmount =
                        overdueDays *
                        config.OverduePenaltyPerDay;

                    var existingFine =
                        await _context.Fines
                            .FirstOrDefaultAsync(f =>
                                f.TransactionID ==
                                transaction.TransactionID);

                    if (existingFine == null)
                    {
                        var fine = new Fine
                        {
                            TransactionID =
                                transaction.TransactionID,
                            Amount = fineAmount,
                            IsPaid = false
                        };

                        _context.Fines.Add(fine);
                    }
                }
            }

            transaction.Status = "Returned";

            var nextReservation = await _context.Reservations
                .Where(r =>
                    r.BookID == transaction.BookID &&
                    r.Status == "Pending")
                .OrderBy(r => r.ReservationDate)
                .FirstOrDefaultAsync();

            if (nextReservation != null)
            {
                nextReservation.Status = "Ready";

                transaction.Book!.AvailabilityStatus =
                    "Reserved";
            }
            else
            {
                transaction.Book!.AvailabilityStatus =
                    "Available";
            }


            await _context.SaveChangesAsync();

            if (overdueDays > 0)
            {
                return (
                    true,
                    $"Book returned. {overdueDays} overdue day(s) recorded.");
            }

            return (true, "Book returned successfully.");
        }


        // ==========================================
        // FULFIL RESERVATION
        // ==========================================

        public async Task<(bool Success, string Message)>
            FulfilReservationAsync(
                int memberId,
                int reservationId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Book)
                .FirstOrDefaultAsync(r =>
                    r.ReservationID == reservationId &&
                    r.MemberID == memberId);

            if (reservation == null)
                return (false, "Reservation not found.");

            if (reservation.Status != "Ready")
                return (false, "This reservation is not ready for collection.");

            if (reservation.Book == null ||
                reservation.Book.AvailabilityStatus != "Reserved")
            {
                return (false, "The reserved book is no longer available.");
            }

            var config = await _context.BorrowingConfigs
                .FirstOrDefaultAsync(c =>
                    c.LibraryID == reservation.Book.LibraryID);

            if (config == null)
                return (false, "Borrowing configuration has not been set for this library.");

            var activeBorrowCount = await _context.BorrowTransactions
                .CountAsync(t =>
                    t.MemberID == memberId &&
                    (t.Status == "Borrowed" ||
                     t.Status == "Overdue"));

            if (activeBorrowCount >= config.MaxBorrowableItems)
            {
                return (
                    false,
                    $"You can borrow a maximum of {config.MaxBorrowableItems} items.");
            }

            var now = DateTime.Now;
            var transaction = new BorrowTransaction
            {
                MemberID = memberId,
                BookID = reservation.BookID,
                BorrowDate = now,
                DueDate = now.AddDays(config.LoanDurationDays),
                Status = "Borrowed",
                RenewalCount = 0
            };

            reservation.Status = "Fulfilled";
            reservation.Book.AvailabilityStatus = "Borrowed";
            _context.BorrowTransactions.Add(transaction);

            await _context.SaveChangesAsync();

            return (true, $"Reservation fulfilled. Due date: {transaction.DueDate:d}");
        }


        // ==========================================
        // RENEW BOOK
        // ==========================================

        public async Task<(bool Success, string Message)>
            RenewBookAsync(
                int memberId,
                int transactionId)
        {
            var transaction =
                await _context.BorrowTransactions
                    .Include(t => t.Book)
                    .FirstOrDefaultAsync(t =>
                        t.TransactionID == transactionId &&
                        t.MemberID == memberId);

            if (transaction == null)
                return (false, "Borrowing transaction not found.");

            if (transaction.Status != "Borrowed")
                return (
                    false,
                    "Only currently borrowed books can be renewed.");

            var config = await _context.BorrowingConfigs
                .FirstOrDefaultAsync(c =>
                    c.LibraryID ==
                    transaction.Book!.LibraryID);

            if (config == null)
                return (
                    false,
                    "Borrowing configuration not found.");

            if (transaction.RenewalCount >=
                config.RenewalLimit)
            {
                return (
                    false,
                    $"Renewal limit of {config.RenewalLimit} has been reached.");
            }

            var hasPendingReservation =
                await _context.Reservations
                    .AnyAsync(r =>
                        r.BookID == transaction.BookID &&
                        r.Status == "Pending" &&
                        r.MemberID != memberId);

            if (hasPendingReservation)
            {
                return (
                    false,
                    "This book has been reserved by another member and cannot be renewed.");
            }

            transaction.DueDate =
                transaction.DueDate
                    .AddDays(config.LoanDurationDays);

            transaction.RenewalCount++;

            await _context.SaveChangesAsync();

            return (
                true,
                $"Book renewed successfully. New due date: {transaction.DueDate:d}");
        }


        // ==========================================
        // UPDATE OVERDUE STATUS
        // ==========================================

        public async Task UpdateOverdueStatusesAsync()
        {
            var overdueTransactions =
                await _context.BorrowTransactions
                    .Include(t => t.Book)
                    .Where(t =>
                        t.Status == "Borrowed" &&
                        t.ReturnDate == null &&
                        t.DueDate < DateTime.Now)
                    .ToListAsync();

            foreach (var transaction in overdueTransactions)
            {
                transaction.Status = "Overdue";

                if (transaction.Book != null)
                {
                    transaction.Book.AvailabilityStatus =
                        "Overdue";
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}