using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace vision.Models
{
    public class ApplyJobViewModel
    {

        [Required]
        public int JobId { get; set; }

        [Required(ErrorMessage = "Please enter your name")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name can only contain letters and spaces")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter your email")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter your qualification")]
        [StringLength(100, ErrorMessage = "Qualification cannot exceed 100 characters")]
        public string Qualification { get; set; }

        [StringLength(500, ErrorMessage = "Message cannot exceed 500 characters")]
        public string Message { get; set; }

        [Required(ErrorMessage = "Please upload your resume")]
        [DataType(DataType.Upload)]
        [AllowedExtensions(new string[] { ".pdf", ".doc", ".docx" })]
        [MaxFileSize(2 * 1024 * 1024, ErrorMessage = "Maximum allowed file size is 2 MB")]
        public IFormFile ResumeFile { get; set; }
    }

}

public class AllowedExtensionsAttribute : ValidationAttribute
{
    private readonly string[] _extensions;

    public AllowedExtensionsAttribute(string[] extensions)
    {
        _extensions = extensions;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var file = value as IFormFile;
        if (file != null)
        {
            var extension = Path.GetExtension(file.FileName);
            if (!_extensions.Contains(extension.ToLower()))
            {
                return new ValidationResult($"File type not allowed. Allowed types are: {string.Join(", ", _extensions)}");
            }
        }
        return ValidationResult.Success;
    }
}

public class MaxFileSizeAttribute : ValidationAttribute
{
    private readonly int _maxFileSize;

    public MaxFileSizeAttribute(int maxFileSize, string errorMessage = null)
    {
        _maxFileSize = maxFileSize;
        ErrorMessage = errorMessage ?? $"Maximum allowed file size is {maxFileSize / (1024 * 1024)} MB";
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var file = value as IFormFile;
        if (file != null && file.Length > _maxFileSize)
        {
            return new ValidationResult(ErrorMessage);
        }

        return ValidationResult.Success;
    }
}


