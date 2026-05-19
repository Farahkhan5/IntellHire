using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vision.Models
{
	public class Company
	{
		public int id { get; set; }

		[Required(ErrorMessage = "Name is required")]
		public string name { get; set; }

		[Required(ErrorMessage = "Tagline is required")]
		public string tagline { get; set; }

		[Required(ErrorMessage = "Category is required")]
		public string category { get; set; }

		[Required(ErrorMessage = "CEO is required")]
		public string CEO { get; set; }

		[Required(ErrorMessage = "please enter your email")]
		[RegularExpression("^[a-zA-Z0-9_.±]+@[a-zA-Z0-9-]+\\.[a-zA-Z0-9-.]+$")]
		public string email { get; set; }

		[Required(ErrorMessage = "Address is required")]
		public string address { get; set; }

		[Required(ErrorMessage = "Description is required")]
		public string description { get; set; }

		[Required(ErrorMessage = "image is required")]
		public string image { get; set; }

		// Link to recruiter who created it
		public int CreatedByRecruiterId { get; set; }

		[ForeignKey(nameof(CreatedByRecruiterId))]
		public recruitersignup Owner { get; set; }

		public DateTime CreatedAt { get; set; }

		// Parameterless constructor for EF
		public Company()
		{
			CreatedAt = DateTime.Now;
		}

		// Convenience constructor (without Owner object)
		public Company(string name, string tagline, string category, string cEO, string email, string address, string description, string image, int createdByRecruiterId)
		{
			this.name = name;
			this.tagline = tagline;
			this.category = category;
			this.CEO = cEO;
			this.email = email;
			this.address = address;
			this.description = description;
			this.image = image;
			this.CreatedByRecruiterId = createdByRecruiterId;
			this.CreatedAt = DateTime.Now;
		}
	}
}
