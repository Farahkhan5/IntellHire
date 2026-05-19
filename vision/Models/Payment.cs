using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace vision.Models
{
    public class Payment
    {
        public int Id { get; set; }

        [Required]
        public int RecruiterId { get; set; }

        [ForeignKey(nameof(RecruiterId))]
        public recruitersignup Recruiter { get; set; }

        [Required(ErrorMessage = "Card number is required")]
        public string CardNumber { get; set; }

        [Required]
        public string Expiry { get; set; }

        [Required]
        public string CVV { get; set; }

        [Required]
        public string CardName { get; set; }

        public int Amount { get; set; }

        public string AdminCard { get; set; }

        public DateTime PaidAt { get; set; }

        public Payment()
        {
            PaidAt = DateTime.Now;
        }
    }
}
