using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class Reservation
    {
        [Key]
        public int ReservationID { get; set; }

        [Required]
        public int MemberID { get; set; }

        public Member? Member { get; set; }

        [Required]
        public int BookID { get; set; }

        public Book? Book { get; set; }

        [Required]
        [Display(Name = "Reservation Date")]
        public DateTime ReservationDate { get; set; } = DateTime.Now;

        [Required]
        public string Status { get; set; } = "Pending";
    }
}