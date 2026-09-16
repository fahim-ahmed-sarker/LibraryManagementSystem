using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class Librarian
    {
        [Key]
        public int LibrarianID { get; set; }

        public string? ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public int LibraryID { get; set; }

        public Library? Library { get; set; }

        public ICollection<BorrowTransaction> ProcessedTransactions { get; set; }
            = new List<BorrowTransaction>();
    }
}