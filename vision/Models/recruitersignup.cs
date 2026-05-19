using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace vision.Models
{
    public class recruitersignup
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter your Name")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "User Name must be between 3 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "User Name must contain only letters and spaces")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter your Email")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter your UserName")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "User Name must be between 3 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "User Name must contain only letters and spaces")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Please enter your Password")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; set; }


        // You must add these:
        public string? ResetToken { get; set; }
        public DateTime? TokenExpiry { get; set; }

        public DateTime CreatedDate { get; set; } // Registration date
        public string? Status { get; set; } // Active / Inactive
        [ValidateNever]
        public ICollection<Job>? Jobs { get; set; } // Navigation property
    }
}
