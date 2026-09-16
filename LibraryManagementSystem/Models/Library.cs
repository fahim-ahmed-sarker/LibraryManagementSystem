using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class Library
    {
        [Key]
        public int LibraryID { get; set; }

        [Required(ErrorMessage = "Library name is required.")]
        [StringLength(100, ErrorMessage = "Library name cannot exceed 100 characters.")]
        [Display(Name = "Library Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters.")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Operating hours are required.")]
        [StringLength(100, ErrorMessage = "Operating hours cannot exceed 100 characters.")]
        [Display(Name = "Operating Hours")]
        public string OperatingHours { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact details are required.")]
        [StringLength(200, ErrorMessage = "Contact details cannot exceed 200 characters.")]
        [Display(Name = "Contact Details")]
        public string ContactDetails { get; set; } = string.Empty;


        // Relationships

        // One Library has many Librarians
        public ICollection<Librarian> Librarians { get; set; }
            = new List<Librarian>();

        // One Library has many Books
        public ICollection<Book> Books { get; set; }
            = new List<Book>();

        // One Library has one Borrowing Configuration
        public BorrowingConfig? BorrowingConfig { get; set; }
    }
}