using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class BorrowingConfig
    {
        [Key]
        public int ConfigID { get; set; }

        [Required]
        [Range(1, 365)]
        [Display(Name = "Loan Duration (Days)")]
        public int LoanDurationDays { get; set; } = 14;

        [Required]
        [Range(0, 20)]
        [Display(Name = "Renewal Limit")]
        public int RenewalLimit { get; set; } = 2;

        [Required]
        [Range(0, 10000)]
        [Display(Name = "Overdue Penalty Per Day")]
        public decimal OverduePenaltyPerDay { get; set; } = 1.00m;

        [Required]
        [Range(1, 100)]
        [Display(Name = "Maximum Borrowable Items")]
        public int MaxBorrowableItems { get; set; } = 5;

        public int LibraryID { get; set; }

        public Library? Library { get; set; }
    }
}