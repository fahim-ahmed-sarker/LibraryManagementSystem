using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Tests
{
    public class BorrowingServiceTests
    {
        // ==========================================
        // CREATE TEST DATABASE
        // ==========================================

        private ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(
                    Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }


        // ==========================================
        // TEST 01
        // BORROW AVAILABLE BOOK
        // ==========================================

        [Fact]
        public async Task BorrowBook_AvailableBook_ShouldSucceed()
        {
            using var context = CreateContext();

            var library = new Library
            {
                Name = "Test Library",
                Location = "Dhaka",
                OperatingHours = "9 AM - 5 PM",
                ContactDetails = "0123456789"
            };

            context.Libraries.Add(library);

            await context.SaveChangesAsync();

            var config = new BorrowingConfig
            {
                LibraryID = library.LibraryID,
                LoanDurationDays = 14,
                RenewalLimit = 2,
                OverduePenaltyPerDay = 10,
                MaxBorrowableItems = 5
            };

            var member = new Member
            {
                Name = "Test Member",
                Email = "member@test.com",
                RegistrationDate = DateTime.Now
            };

            var book = new Book
            {
                Title = "Test Book",
                Author = "Test Author",
                Genre = "Fiction",
                ISBN = "1234567890",
                Summary = "Test summary",
                AvailabilityStatus = "Available",
                LibraryID = library.LibraryID
            };

            context.BorrowingConfigs.Add(config);
            context.Members.Add(member);
            context.Books.Add(book);

            await context.SaveChangesAsync();

            var service =
                new BorrowingService(context);

            var result =
                await service.BorrowBookAsync(
                    member.MemberID,
                    book.BookID);

            Assert.True(result.Success);

            Assert.Equal(
                "Borrowed",
                book.AvailabilityStatus);

            Assert.Equal(
                1,
                await context.BorrowTransactions.CountAsync());
        }


        // ==========================================
        // TEST 02
        // BORROW UNAVAILABLE BOOK
        // ==========================================

        [Fact]
        public async Task BorrowBook_UnavailableBook_ShouldFail()
        {
            using var context = CreateContext();

            var library = new Library
            {
                Name = "Test Library",
                Location = "Dhaka",
                OperatingHours = "9 AM - 5 PM",
                ContactDetails = "0123456789"
            };

            context.Libraries.Add(library);

            await context.SaveChangesAsync();

            var config = new BorrowingConfig
            {
                LibraryID = library.LibraryID,
                LoanDurationDays = 14,
                RenewalLimit = 2,
                OverduePenaltyPerDay = 10,
                MaxBorrowableItems = 5
            };

            var member = new Member
            {
                Name = "Test Member",
                Email = "member@test.com"
            };

            var book = new Book
            {
                Title = "Unavailable Book",
                Author = "Author",
                Genre = "Fiction",
                ISBN = "1234567891",
                Summary = "Summary",
                AvailabilityStatus = "Borrowed",
                LibraryID = library.LibraryID
            };

            context.BorrowingConfigs.Add(config);
            context.Members.Add(member);
            context.Books.Add(book);

            await context.SaveChangesAsync();

            var service =
                new BorrowingService(context);

            var result =
                await service.BorrowBookAsync(
                    member.MemberID,
                    book.BookID);

            Assert.False(result.Success);

            Assert.Contains(
                "not currently available",
                result.Message);
        }


        // ==========================================
        // TEST 03
        // MAXIMUM BORROWING LIMIT
        // ==========================================

        [Fact]
        public async Task BorrowBook_MaximumLimitReached_ShouldFail()
        {
            using var context = CreateContext();

            var library = new Library
            {
                Name = "Test Library",
                Location = "Dhaka",
                OperatingHours = "9 AM - 5 PM",
                ContactDetails = "0123456789"
            };

            context.Libraries.Add(library);

            await context.SaveChangesAsync();

            var config = new BorrowingConfig
            {
                LibraryID = library.LibraryID,
                LoanDurationDays = 14,
                RenewalLimit = 2,
                OverduePenaltyPerDay = 10,
                MaxBorrowableItems = 1
            };

            var member = new Member
            {
                Name = "Test Member",
                Email = "member@test.com"
            };

            var existingBook = new Book
            {
                Title = "Existing Book",
                Author = "Author",
                Genre = "Fiction",
                ISBN = "1234567892",
                Summary = "Summary",
                AvailabilityStatus = "Borrowed",
                LibraryID = library.LibraryID
            };

            var newBook = new Book
            {
                Title = "New Book",
                Author = "Author",
                Genre = "Fiction",
                ISBN = "1234567893",
                Summary = "Summary",
                AvailabilityStatus = "Available",
                LibraryID = library.LibraryID
            };

            context.BorrowingConfigs.Add(config);
            context.Members.Add(member);
            context.Books.Add(existingBook);
            context.Books.Add(newBook);

            await context.SaveChangesAsync();

            context.BorrowTransactions.Add(
                new BorrowTransaction
                {
                    MemberID = member.MemberID,
                    BookID = existingBook.BookID,
                    BorrowDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(14),
                    Status = "Borrowed"
                });

            await context.SaveChangesAsync();

            var service =
                new BorrowingService(context);

            var result =
                await service.BorrowBookAsync(
                    member.MemberID,
                    newBook.BookID);

            Assert.False(result.Success);

            Assert.Contains(
                "maximum",
                result.Message);
        }


        // ==========================================
        // TEST 04
        // RETURN BOOK
        // ==========================================

        [Fact]
        public async Task ReturnBook_BorrowedBook_ShouldSucceed()
        {
            using var context = CreateContext();

            var library = new Library
            {
                Name = "Test Library",
                Location = "Dhaka",
                OperatingHours = "9 AM - 5 PM",
                ContactDetails = "0123456789"
            };

            context.Libraries.Add(library);

            await context.SaveChangesAsync();

            var config = new BorrowingConfig
            {
                LibraryID = library.LibraryID,
                LoanDurationDays = 14,
                RenewalLimit = 2,
                OverduePenaltyPerDay = 10,
                MaxBorrowableItems = 5
            };

            var member = new Member
            {
                Name = "Test Member",
                Email = "member@test.com"
            };

            var book = new Book
            {
                Title = "Return Test",
                Author = "Author",
                Genre = "Fiction",
                ISBN = "1234567894",
                Summary = "Summary",
                AvailabilityStatus = "Borrowed",
                LibraryID = library.LibraryID
            };

            context.BorrowingConfigs.Add(config);
            context.Members.Add(member);
            context.Books.Add(book);

            await context.SaveChangesAsync();

            var transaction = new BorrowTransaction
            {
                MemberID = member.MemberID,
                BookID = book.BookID,
                BorrowDate = DateTime.Now.AddDays(-2),
                DueDate = DateTime.Now.AddDays(12),
                Status = "Borrowed"
            };

            context.BorrowTransactions.Add(transaction);

            await context.SaveChangesAsync();

            var service =
                new BorrowingService(context);

            var result =
                await service.ReturnBookAsync(
                    member.MemberID,
                    transaction.TransactionID);

            Assert.True(result.Success);

            Assert.Equal(
                "Returned",
                transaction.Status);

            Assert.Equal(
                "Available",
                book.AvailabilityStatus);

            Assert.NotNull(transaction.ReturnDate);
        }


        // ==========================================
        // TEST 05
        // OVERDUE FINE
        // ==========================================

        [Fact]
        public async Task ReturnBook_OverdueBook_ShouldCreateFine()
        {
            using var context = CreateContext();

            var library = new Library
            {
                Name = "Test Library",
                Location = "Dhaka",
                OperatingHours = "9 AM - 5 PM",
                ContactDetails = "0123456789"
            };

            context.Libraries.Add(library);

            await context.SaveChangesAsync();

            var config = new BorrowingConfig
            {
                LibraryID = library.LibraryID,
                LoanDurationDays = 14,
                RenewalLimit = 2,
                OverduePenaltyPerDay = 10,
                MaxBorrowableItems = 5
            };

            var member = new Member
            {
                Name = "Test Member",
                Email = "member@test.com"
            };

            var book = new Book
            {
                Title = "Overdue Test",
                Author = "Author",
                Genre = "Fiction",
                ISBN = "1234567895",
                Summary = "Summary",
                AvailabilityStatus = "Overdue",
                LibraryID = library.LibraryID
            };

            context.BorrowingConfigs.Add(config);
            context.Members.Add(member);
            context.Books.Add(book);

            await context.SaveChangesAsync();

            var transaction = new BorrowTransaction
            {
                MemberID = member.MemberID,
                BookID = book.BookID,
                BorrowDate = DateTime.Now.AddDays(-20),
                DueDate = DateTime.Now.AddDays(-5),
                Status = "Overdue"
            };

            context.BorrowTransactions.Add(transaction);

            await context.SaveChangesAsync();

            var service =
                new BorrowingService(context);

            var result =
                await service.ReturnBookAsync(
                    member.MemberID,
                    transaction.TransactionID);

            Assert.True(result.Success);

            var fine = await context.Fines
                .FirstOrDefaultAsync(f =>
                    f.TransactionID ==
                    transaction.TransactionID);

            Assert.NotNull(fine);

            Assert.Equal(
                50m,
                fine!.Amount);

            Assert.False(fine.IsPaid);
        }


        // ==========================================
        // TEST 06
        // RENEWAL LIMIT
        // ==========================================

        [Fact]
        public async Task RenewBook_RenewalLimitReached_ShouldFail()
        {
            using var context = CreateContext();

            var library = new Library
            {
                Name = "Test Library",
                Location = "Dhaka",
                OperatingHours = "9 AM - 5 PM",
                ContactDetails = "0123456789"
            };

            context.Libraries.Add(library);

            await context.SaveChangesAsync();

            var config = new BorrowingConfig
            {
                LibraryID = library.LibraryID,
                LoanDurationDays = 14,
                RenewalLimit = 2,
                OverduePenaltyPerDay = 10,
                MaxBorrowableItems = 5
            };

            var member = new Member
            {
                Name = "Test Member",
                Email = "member@test.com"
            };

            var book = new Book
            {
                Title = "Renewal Test",
                Author = "Author",
                Genre = "Fiction",
                ISBN = "1234567896",
                Summary = "Summary",
                AvailabilityStatus = "Borrowed",
                LibraryID = library.LibraryID
            };

            context.BorrowingConfigs.Add(config);
            context.Members.Add(member);
            context.Books.Add(book);

            await context.SaveChangesAsync();

            var transaction = new BorrowTransaction
            {
                MemberID = member.MemberID,
                BookID = book.BookID,
                BorrowDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(14),
                Status = "Borrowed",
                RenewalCount = 2
            };

            context.BorrowTransactions.Add(transaction);

            await context.SaveChangesAsync();

            var service =
                new BorrowingService(context);

            var result =
                await service.RenewBookAsync(
                    member.MemberID,
                    transaction.TransactionID);

            Assert.False(result.Success);

            Assert.Contains(
                "Renewal limit",
                result.Message);
        }


        [Fact]
        public async Task ReturnBookForLibrarian_BorrowedBook_ShouldSucceed()
        {
            using var context = CreateContext();

            var library = new Library { Name = "Test Library" };
            var member = new Member { Name = "Member" };
            var book = new Book
            {
                Title = "Librarian Return",
                Author = "Author",
                Genre = "Fiction",
                ISBN = "1234567897",
                Summary = "Summary",
                AvailabilityStatus = "Borrowed",
                Library = library
            };

            context.Libraries.Add(library);
            context.Members.Add(member);
            context.Books.Add(book);
            await context.SaveChangesAsync();

            var transaction = new BorrowTransaction
            {
                MemberID = member.MemberID,
                BookID = book.BookID,
                BorrowDate = DateTime.Now.AddDays(-1),
                DueDate = DateTime.Now.AddDays(10),
                Status = "Borrowed"
            };

            context.BorrowTransactions.Add(transaction);
            await context.SaveChangesAsync();

            var service = new BorrowingService(context);
            var result = await service.ReturnBookForLibrarianAsync(
                transaction.TransactionID);

            Assert.True(result.Success);
            Assert.Equal("Returned", transaction.Status);
            Assert.Equal("Available", book.AvailabilityStatus);
        }


        [Fact]
        public async Task FulfilReservation_ReadyReservation_ShouldCreateBorrowing()
        {
            using var context = CreateContext();

            var library = new Library { Name = "Test Library" };
            var config = new BorrowingConfig
            {
                Library = library,
                LoanDurationDays = 14,
                RenewalLimit = 2,
                OverduePenaltyPerDay = 1,
                MaxBorrowableItems = 5
            };
            var member = new Member { Name = "Member" };
            var book = new Book
            {
                Title = "Reserved Book",
                Author = "Author",
                Genre = "Fiction",
                ISBN = "1234567898",
                Summary = "Summary",
                AvailabilityStatus = "Reserved",
                Library = library
            };

            context.Libraries.Add(library);
            context.BorrowingConfigs.Add(config);
            context.Members.Add(member);
            context.Books.Add(book);
            await context.SaveChangesAsync();

            var reservation = new Reservation
            {
                MemberID = member.MemberID,
                BookID = book.BookID,
                Status = "Ready"
            };

            context.Reservations.Add(reservation);
            await context.SaveChangesAsync();

            var service = new BorrowingService(context);
            var result = await service.FulfilReservationAsync(
                member.MemberID,
                reservation.ReservationID);

            Assert.True(result.Success);
            Assert.Equal("Fulfilled", reservation.Status);
            Assert.Equal("Borrowed", book.AvailabilityStatus);
            Assert.Single(context.BorrowTransactions);
        }
    }
}