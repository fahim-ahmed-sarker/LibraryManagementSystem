using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class Feedback
    {
        [Key]
        public int FeedbackID { get; set; }

        [Required]
        public int MemberID { get; set; }

        public Member? Member { get; set; }

        [Required]
        public int BookID { get; set; }

        public Book? Book { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(1000)]
        public string? Comment { get; set; }

        [Display(Name = "Date Submitted")]
        public DateTime DateSubmitted { get; set; } = DateTime.Now;
    }
}