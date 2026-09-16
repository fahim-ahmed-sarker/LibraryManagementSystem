using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class Fine
    {
        [Key]
        public int FineID { get; set; }

        [Required]
        public int TransactionID { get; set; }

        public BorrowTransaction? Transaction { get; set; }

        [Required]
        [Range(0, 100000)]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Display(Name = "Paid")]
        public bool IsPaid { get; set; } = false;

        [Display(Name = "Paid Date")]
        public DateTime? PaidDate { get; set; }
    }
}