using System.ComponentModel.DataAnnotations;

namespace vision.Models
{
    public class register
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter your User Name")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "User Name must be between 3 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "User Name must contain only letters and spaces")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter your Email")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter your Password")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; set; }

        public string? ProfileImagePath { get; set; }

        public string? ResetToken { get; set; }
        public DateTime? TokenExpiry { get; set; }


        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Active";
    }
}
