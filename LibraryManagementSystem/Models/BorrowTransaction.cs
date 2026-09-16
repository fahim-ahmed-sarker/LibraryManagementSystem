using System.ComponentModel.DataAnnotations;
using static System.Net.WebRequestMethods;

namespace LibraryManagementSystem.Models
{
    public class BorrowTransaction
    {
        [Key]
        public int TransactionID { get; set; }

        [Required]
        public int MemberID { get; set; }

        public Member? Member { get; set; }

        [Required]
        public int BookID { get; set; }

        public Book? Book { get; set; }

        // Nullable because online borrowing may occur
        // without a librarian directly processing it.
        public int? LibrarianID { get; set; }

        public Librarian? Librarian { get; set; }

        [Required]
        [Display(Name = "Borrow Date")]
        public DateTime BorrowDate { get; set; }

        [Required]
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; }

        [Display(Name = "Return Date")]
        public DateTime? ReturnDate { get; set; }

        [Required]
        public string Status { get; set; } = "Borrowed";

        [Range(0, 20)]
        public int RenewalCount { get; set; } = 0;

        public Fine? Fine { get; set; }
    }
}