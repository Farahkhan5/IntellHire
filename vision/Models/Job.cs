using System.ComponentModel.DataAnnotations;

namespace vision.Models
{
	public class Job
	{
		public int id { get; set; }

		[Required(ErrorMessage = "Job Title is required")]
		public string title { get; set; }

		[Required(ErrorMessage = "Please enter your email")]
		[RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$", ErrorMessage = "Invalid email format")]
		public string email { get; set; }

		[Required(ErrorMessage = "Job Type is required")]
		public string type { get; set; }

		[Required(ErrorMessage = "Address is required")]
		public string address { get; set; }

		[Required(ErrorMessage = "Qualifications is required")]
		public string qualifications { get; set; }

		[Required(ErrorMessage = "Salary is required")]
		public string salary { get; set; }

		[Required(ErrorMessage = "Responsibility is required")]
		public string responsibility { get; set; }

		[Required(ErrorMessage = "description is required")]
		public string description { get; set; }

		public DateTime CreatedAt { get; set; }

		public int RecruiterId { get; set; } // FK to recruitersignup

		// Link to company (optional)
		public int? CompanyId { get; set; }

		// Made nullable so it's not implicitly required by validation
		public Company? Company { get; set; }

		public Job(int id, string title, string email, string type, string address, string qualifications, string salary, string responsibility, string description, DateTime createdAt)
		{
			this.id = id;
			this.title = title;
			this.email = email;
			this.type = type;
			this.address = address;
			this.qualifications = qualifications;
			this.salary = salary;
			this.responsibility = responsibility;
			this.description = description;
			CreatedAt = createdAt;
		}
		public Job() { }
	}

}
