using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Library> Libraries { get; set; }

        public DbSet<Librarian> Librarians { get; set; }

        public DbSet<Member> Members { get; set; }

        public DbSet<Book> Books { get; set; }

        public DbSet<BorrowingConfig> BorrowingConfigs { get; set; }

        public DbSet<BorrowTransaction> BorrowTransactions { get; set; }

        public DbSet<Reservation> Reservations { get; set; }

        public DbSet<Fine> Fines { get; set; }

        public DbSet<Feedback> Feedbacks { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Member>()
    .HasOne(m => m.ApplicationUser)
    .WithOne()
    .HasForeignKey<Member>(m => m.ApplicationUserId)
    .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Librarian>()
                .HasOne(l => l.ApplicationUser)
                .WithOne()
                .HasForeignKey<Librarian>(l => l.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Library 1 : 1 BorrowingConfig
            builder.Entity<Library>()
                .HasOne(l => l.BorrowingConfig)
                .WithOne(c => c.Library)
                .HasForeignKey<BorrowingConfig>(c => c.LibraryID)
                .OnDelete(DeleteBehavior.Cascade);


            // Library 1 : Many Librarians
            builder.Entity<Librarian>()
                .HasOne(l => l.Library)
                .WithMany(l => l.Librarians)
                .HasForeignKey(l => l.LibraryID)
                .OnDelete(DeleteBehavior.Restrict);


            // Library 1 : Many Books
            builder.Entity<Book>()
                .HasOne(b => b.Library)
                .WithMany(l => l.Books)
                .HasForeignKey(b => b.LibraryID)
                .OnDelete(DeleteBehavior.Restrict);


            // Member 1 : Many BorrowTransactions
            builder.Entity<BorrowTransaction>()
                .HasOne(t => t.Member)
                .WithMany(m => m.Transactions)
                .HasForeignKey(t => t.MemberID)
                .OnDelete(DeleteBehavior.Restrict);


            // Book 1 : Many BorrowTransactions
            builder.Entity<BorrowTransaction>()
                .HasOne(t => t.Book)
                .WithMany(b => b.Transactions)
                .HasForeignKey(t => t.BookID)
                .OnDelete(DeleteBehavior.Restrict);


            // Librarian 1 : Many BorrowTransactions
            // Nullable because online borrowing is supported.
            builder.Entity<BorrowTransaction>()
                .HasOne(t => t.Librarian)
                .WithMany(l => l.ProcessedTransactions)
                .HasForeignKey(t => t.LibrarianID)
                .OnDelete(DeleteBehavior.SetNull);


            // BorrowTransaction 1 : 0..1 Fine
            builder.Entity<BorrowTransaction>()
                .HasOne(t => t.Fine)
                .WithOne(f => f.Transaction)
                .HasForeignKey<Fine>(f => f.TransactionID)
                .OnDelete(DeleteBehavior.Cascade);


            // Member 1 : Many Reservations
            builder.Entity<Reservation>()
                .HasOne(r => r.Member)
                .WithMany(m => m.Reservations)
                .HasForeignKey(r => r.MemberID)
                .OnDelete(DeleteBehavior.Restrict);


            // Book 1 : Many Reservations
            builder.Entity<Reservation>()
                .HasOne(r => r.Book)
                .WithMany(b => b.Reservations)
                .HasForeignKey(r => r.BookID)
                .OnDelete(DeleteBehavior.Restrict);


            // Member 1 : Many Feedback
            builder.Entity<Feedback>()
                .HasOne(f => f.Member)
                .WithMany(m => m.Feedbacks)
                .HasForeignKey(f => f.MemberID)
                .OnDelete(DeleteBehavior.Restrict);


            // Book 1 : Many Feedback
            builder.Entity<Feedback>()
                .HasOne(f => f.Book)
                .WithMany(b => b.Feedbacks)
                .HasForeignKey(f => f.BookID)
                .OnDelete(DeleteBehavior.Restrict);


            // Decimal precision
            builder.Entity<BorrowingConfig>()
                .Property(c => c.OverduePenaltyPerDay)
                .HasPrecision(18, 2);

            builder.Entity<Fine>()
                .Property(f => f.Amount)
                .HasPrecision(18, 2);


            // ISBN index
            builder.Entity<Book>()
                .HasIndex(b => b.ISBN)
                .IsUnique();


            // One feedback from a member for a particular book
            builder.Entity<Feedback>()
                .HasIndex(f => new { f.MemberID, f.BookID })
                .IsUnique();
        }
    }
}