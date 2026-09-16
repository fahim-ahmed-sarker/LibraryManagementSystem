using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class Book
    {
        [Key]
        public int BookID { get; set; }

        [Required(ErrorMessage = "Book title is required.")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Author is required.")]
        [StringLength(150)]
        public string Author { get; set; } = string.Empty;

        [Required(ErrorMessage = "Genre is required.")]
        [StringLength(100)]
        public string Genre { get; set; } = string.Empty;

        [Required(ErrorMessage = "ISBN is required.")]
        [RegularExpression(
            @"^(?:\d{10}|\d{13})$",
            ErrorMessage = "ISBN must contain exactly 10 or 13 digits.")]
        public string ISBN { get; set; } = string.Empty;

        [Required(ErrorMessage = "Book summary is required.")]
        [StringLength(2000)]
        public string Summary { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Availability Status")]
        public string AvailabilityStatus { get; set; } = "Available";

        [Display(Name = "Cover Image")]
        public string? CoverImageUrl { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.Now;

        public int LibraryID { get; set; }

        public Library? Library { get; set; }

        public ICollection<BorrowTransaction> Transactions { get; set; }
            = new List<BorrowTransaction>();

        public ICollection<Reservation> Reservations { get; set; }
            = new List<Reservation>();

        public ICollection<Feedback> Feedbacks { get; set; }
            = new List<Feedback>();
    }
}