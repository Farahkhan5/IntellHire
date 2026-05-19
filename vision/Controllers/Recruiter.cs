using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Xml.Linq;
using vision.Models;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace vision.Controllers
{
    public class Recruiter : Controller
    {
        Connection con = new Connection();

		// Helper to get current recruiter object from session
		private recruitersignup GetCurrentRecruiter()
		{
			var recruiterId = HttpContext.Session.GetInt32("recruiterId");
			if (recruiterId == null)
				return null;

			var recruiter = con.signup.FirstOrDefault(a => a.Id == recruiterId);
			return recruiter;
		}


		public IActionResult Index()
        {
            return View();
        }

		/////   Job Start


		[HttpGet]
		public IActionResult Job()
		{
			var recruiter = GetCurrentRecruiter();
			if (recruiter == null)
				return RedirectToAction("Login");

			int recruiterId = recruiter.Id; // or .Id

			var companies = con.Company.Where(c => c.CreatedByRecruiterId == recruiterId).ToList();
			ViewBag.Companies = new SelectList(companies, "id", "name");

			return View(new Job());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Job(Job job)
		{
			var recruiter = GetCurrentRecruiter();
			if (recruiter == null)
				return RedirectToAction("Login");

			int recruiterId = recruiter.Id;

			var companies = con.Company
				.Where(c => c.CreatedByRecruiterId == recruiterId)
				.ToList();
			ViewBag.Companies = new SelectList(companies, "id", "name", job.CompanyId);

			job.RecruiterId = recruiter.Id; // ✅ set the recruiterId before saving
			job.CreatedAt = DateTime.Now;

			if (!ModelState.IsValid)
			{
				ViewBag.ValidationErrors = ModelState
					.Where(kv => kv.Value.Errors.Count > 0)
					.SelectMany(kv => kv.Value.Errors.Select(e => $"{kv.Key}: {e.ErrorMessage}"))
					.ToList();
				return View(job);
			}

			con.jobs.Add(job);
			con.SaveChanges();
			return RedirectToAction("Job_detail");
		}

		public IActionResult Job_detail()
		{
			var recruiter = GetCurrentRecruiter();
			if (recruiter == null)
				return RedirectToAction("Login");

			int recruiterId = recruiter.Id;

			List<Job> data;
			try
			{
				data = con.jobs
					.Include(j => j.Company)
					.Where(j => j.Company.CreatedByRecruiterId == recruiterId)
					.ToList();
			}
			catch
			{
				data = new List<Job>();
			}

			return View(data);
		}

		public IActionResult Job_Edit(int id)
		{
			var recruiter = GetCurrentRecruiter();
			if (recruiter == null) return RedirectToAction("Login");

			int recruiterId = recruiter.Id;

			var data = con.jobs
				.Include(j => j.Company)
				.FirstOrDefault(x => x.id == id && x.Company.CreatedByRecruiterId == recruiterId);

			if (data == null) return NotFound(); // Not your job or doesn't exist

			var companies = con.Company
				.Where(c => c.CreatedByRecruiterId == recruiterId)
				.ToList();

			ViewBag.Companies = new SelectList(companies, "id", "name", data.CompanyId);

			return View(data);
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Job_Edit(int id, Job updated)
		{
			var recruiter = GetCurrentRecruiter();
			if (recruiter == null) return RedirectToAction("Login");

			int recruiterId = recruiter.Id;

			var data = con.jobs
				.Include(j => j.Company)
				.FirstOrDefault(x => x.id == id && x.Company.CreatedByRecruiterId == recruiterId);

			if (data == null) return NotFound(); // Not your job or doesn't exist

			// Update fields
			data.title = updated.title;
			data.email = updated.email;
			data.type = updated.type;
			data.address = updated.address;
			data.qualifications = updated.qualifications;
			data.salary = updated.salary;
			data.responsibility = updated.responsibility;
			data.description = updated.description;
			data.CompanyId = updated.CompanyId;
			data.CreatedAt = DateTime.Now;

			// Only show companies created by the same recruiter
			var companies = con.Company
				.Where(c => c.CreatedByRecruiterId == recruiterId)
				.ToList();
			ViewBag.Companies = new SelectList(companies, "id", "name", data.CompanyId);

			if (!TryValidateModel(data))
				return View(data);

			con.SaveChanges();
			return RedirectToAction("Job_detail");
		}

		public IActionResult Job_Delete(int id)
		{
			var recruiter = GetCurrentRecruiter();
			if (recruiter == null) return RedirectToAction("Login");

			var job = con.jobs
				.Include(j => j.Company)
				.FirstOrDefault(j => j.id == id && j.Company.CreatedByRecruiterId == recruiter.Id);

			if (job == null) return NotFound(); // Not your job or doesn't exist

			con.jobs.Remove(job);
			con.SaveChanges();

			return RedirectToAction("Job_detail");
		}


        /////   Job End

        public IActionResult Pricing()
        {
            // Example: Fetch % commission from config/db
            ViewBag.CommissionPercent = 10; // Suppose 10%
            return View();
        }

        [HttpPost]
        public IActionResult AcceptPricing()
        {
            // ✅ Mark recruiter as accepted
            var recruiter = GetCurrentRecruiter();
            if (recruiter == null) return RedirectToAction("Login");

            // Save in session or DB
            HttpContext.Session.SetString("AcceptedPricing", "true");

            // Redirect to billing form
            return RedirectToAction("Billing");
        }

        public IActionResult Billing()
        {
            if (HttpContext.Session.GetString("AcceptedPricing") != "true")
                return RedirectToAction("Pricing");

            ViewBag.AdminCard = "4111 1111 1111 1111"; // ✅ fixed card number (Admin side)
            return View();
        }

        [HttpPost]
        public IActionResult Billing(BillingViewModel model)
        {
            var recruiter = GetCurrentRecruiter();
            if (recruiter == null) return RedirectToAction("Login");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // ✅ Expiry validation
            try
            {
                var parts = model.Expiry.Split('/');
                int month = int.Parse(parts[0]);
                int year = int.Parse(parts[1]) + 2000; // convert YY -> YYYY

                var expiryDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));

                if (expiryDate < DateTime.Now)
                {
                    ModelState.AddModelError("Expiry", "Expiry date cannot be in the past.");
                    return View(model);
                }
            }
            catch
            {
                ModelState.AddModelError("Expiry", "Invalid expiry date format.");
                return View(model);
            }

            // ✅ Dummy payment success
            bool paymentSuccess = true;

            if (paymentSuccess)
            {
                Payment payment = new Payment
                {
                    RecruiterId = recruiter.Id,
                    CardNumber = model.CardNumber,
                    Expiry = model.Expiry,
                    CVV = model.CVV,
                    CardName = model.CardName,
                    Amount = 1000,
                    AdminCard = "4111 1111 1111 1111"
                };

                con.Payments.Add(payment);
                con.SaveChanges();

                HttpContext.Session.SetString("PaymentDone", "true");
                return RedirectToAction("Company");
            }

            ModelState.AddModelError("", "Payment failed. Please try again.");
            return View(model);
        }

        public IActionResult Payments()
        {
            var payments = con.Payments
                .Include(p => p.Recruiter)
                .OrderByDescending(p => p.PaidAt)
                .ToList();

            return View(payments);
        }




        /////   Company Start
        [HttpGet]
        public IActionResult Company()
        {
            if (HttpContext.Session.GetString("PaymentDone") != "true")
                return RedirectToAction("Billing");

            return View(); // company create form
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Company(string name, string tagline, string category, string cEO, string email, string address, string description, IFormFile image)
        {
            var recruiter = GetCurrentRecruiter();
            if (recruiter == null)
                return RedirectToAction("Login");

            if (HttpContext.Session.GetString("PaymentDone") != "true")
            {
                // Agar payment nahi hui hai, wapas Billing par bhej do
                return RedirectToAction("Billing");
            }

            int recruiterId = recruiter.Id;
            bool hasError = false;

            if (con.Company.Any(c => c.name == name && c.CreatedByRecruiterId == recruiterId))
            {
                ModelState.AddModelError("name", "You already have a company with this name.");
                hasError = true;
            }

            if (image != null && con.Company.Any(c => c.image == image.FileName && c.CreatedByRecruiterId == recruiterId))
            {
                ModelState.AddModelError("image", "You have already used this image for your company.");
                hasError = true;
            }

            // ✅ Image extension check
            if (image != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var ext = Path.GetExtension(image.FileName).ToLower();

                if (!allowedExtensions.Contains(ext))
                {
                    ModelState.AddModelError("image", "Only .jpg, .jpeg, .png files are allowed.");
                    hasError = true;
                }
            }

            if (!hasError && ModelState.IsValid)
            {
                string img_name = string.Empty;
                if (image != null)
                {
                    img_name = Path.GetFileName(image.FileName);
                    var img_path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", img_name);
                    using (FileStream stream = new FileStream(img_path, FileMode.Create))
                    {
                        image.CopyTo(stream);
                    }
                }

                Company cm = new Company(
                    name: name,
                    tagline: tagline,
                    category: category,
                    cEO: cEO,
                    email: email,
                    address: address,
                    description: description,
                    image: img_name,
                    createdByRecruiterId: recruiterId
                );

                con.Company.Add(cm);
                con.SaveChanges();

                HttpContext.Session.SetString("PaymentDone", "false");


                return RedirectToAction("M_Company");
            }

            return View();
        }

        public IActionResult Company_Delete(int id)
		{
			var recruiter = GetCurrentRecruiter();
			if (recruiter == null)
				return RedirectToAction("Login");

			var data = con.Company.FirstOrDefault(x => x.id == id && x.CreatedByRecruiterId == recruiter.Id);
			if (data == null)
				return NotFound(); // Company doesn't exist or doesn't belong to this recruiter

			con.Company.Remove(data);
			con.SaveChanges();

			return RedirectToAction("M_Company");
		}

		public IActionResult Company_Edit(int id)
		{
			var recruiter = GetCurrentRecruiter();
			if (recruiter == null)
				return RedirectToAction("Login");

			var data = con.Company.FirstOrDefault(x => x.id == id && x.CreatedByRecruiterId == recruiter.Id);
			if (data == null)
				return NotFound(); // Not your company or doesn't exist

			return View(data);
		}

		[HttpPost]
		public IActionResult Company_Edit(int id, string name, string tagline, string category, string cEO, string email, string address, string description, IFormFile? image, string OldImage)
		{
			var recruiter = GetCurrentRecruiter();
			if (recruiter == null)
				return RedirectToAction("Login");

			var data = con.Company.FirstOrDefault(x => x.id == id && x.CreatedByRecruiterId == recruiter.Id);
			if (data == null)
				return NotFound(); // Not your company or doesn't exist

			string image_name = OldImage;

			if (image != null && image.Length > 0)
			{
				image_name = Path.GetFileName(image.FileName);
				var img_path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", image_name);

				using (FileStream stream = new FileStream(img_path, FileMode.Create))
				{
					image.CopyTo(stream);
				}
			}

			// Update fields
			data.image = image_name;
			data.name = name;
			data.tagline = tagline;
			data.category = category;
			data.CEO = cEO;
			data.email = email;
			data.address = address;
			data.description = description;

			con.SaveChanges();

			return RedirectToAction("M_Company");
		}


		///  Company End



		public IActionResult M_Company()
		{
			var recruiter = GetCurrentRecruiter();
			if (recruiter == null)
				return RedirectToAction("Login");

			int recruiterId = recruiter.Id;

			var myCompanies = con.Company
								 .Where(c => c.CreatedByRecruiterId == recruiterId)
								 .ToList();

			return View(myCompanies);
		}


		public IActionResult Candidate()
		{
			return View();
		}


		// GET: /Recruiter/Employee
		[HttpGet]
		public IActionResult Employee()
		{
			var recruiter = GetCurrentRecruiter();
			if (recruiter == null)
				return RedirectToAction("Login", "Recruiter");

			int recruiterId = recruiter.Id;

			var applications = (from a in con.ApplyJobs
								join j in con.jobs on a.JobId equals j.id
								join p in con.register on a.Email equals p.Email into profileJoin
								from p in profileJoin.DefaultIfEmpty() // left join for optional profile
								where j.RecruiterId == recruiterId
								select new AppliedCandidateViewModel
								{
									Id = a.Id,
									Name = a.Name,
									JobTitle = j.title,
									ResumeFilePath = a.ResumeFilePath,
									ApplicationStatus = a.ApplicationStatus,
									ProfileImagePath = p.ProfileImagePath // may be null
								}).ToList();

			return View(applications);
		}


		[HttpPost]
		public IActionResult UpdateApplicationStatus(int id, string status)
		{
			var candidate = con.ApplyJobs.FirstOrDefault(c => c.Id == id);
			if (candidate != null)
			{
				candidate.ApplicationStatus = status;
				con.SaveChanges();
				return Ok();
			}
			return BadRequest();
		}

		public IActionResult Resume()
		{
			return View();
		}
        [HttpGet]
        public IActionResult Signup()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Signup(recruitersignup signup)
        {
            if (ModelState.IsValid)
            {
                con.signup.Add(signup);
                con.SaveChanges();
                return RedirectToAction("Login");
            }

            return View(signup);
        }
        public IActionResult Login()
        {
            return View();
        }
		[HttpPost]
		public IActionResult Login(recruitersignup signup)
		{
			var login = con.signup.FirstOrDefault(a => a.Username == signup.Username && a.Password == signup.Password);
			if (login != null)
			{
				HttpContext.Session.SetInt32("recruiterId", login.Id); // ✅ set recruiterId (not Name)
				return RedirectToAction("Index");
			}

			ViewBag.Error = "Invalid Username or Password";
			return View();
		}

		public IActionResult Logout()
		{
			HttpContext.Session.Clear(); // 🔓 Clear session
			return RedirectToAction("Login");
		}
        // GET: Show Forget Password form
        [HttpGet]
        public IActionResult Lost_password()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Lost_password(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Please enter your email address.";
                return View();
            }

            var user = con.signup.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                // Generate reset token
                user.ResetToken = Guid.NewGuid().ToString();
                user.TokenExpiry = DateTime.Now.AddHours(1);
                con.SaveChanges();

                // Create reset link
                var resetLink = Url.Action("ResetPassword", "Recruiter", new { token = user.ResetToken }, Request.Scheme);

                // Send email
                var subject = "Reset Your Password";
                var body = $"Click here to reset your password: <a href='{resetLink}'>Reset Password</a>";

                SendEmail(user.Email, subject, body); // your email helper

                ViewBag.Message = "Reset link has been sent to your email.";
            }
            else
            {
                ViewBag.Message = "If this email is registered, a reset link has been sent.";
            }

            return View();
        }



        // GET: Reset password page
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Lost_password");

            var recruiter = con.signup.FirstOrDefault(r => r.ResetToken == token && r.TokenExpiry > DateTime.Now);
            if (recruiter == null)
            {
                TempData["Error"] = "Invalid or expired token.";
                return RedirectToAction("Lost_password");
            }

            // Pass token to view
            return View(model: token);
        }

        // POST: Update password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(string token, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Lost_password");

            var recruiter = con.signup.FirstOrDefault(r => r.ResetToken == token && r.TokenExpiry > DateTime.Now);
            if (recruiter == null)
            {
                TempData["Error"] = "Invalid or expired token.";
                return RedirectToAction("Lost_password");
            }

            if (string.IsNullOrEmpty(newPassword) || newPassword != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View(model: token);
            }

            // Update password (hash if needed)
            recruiter.Password = newPassword;
            recruiter.ResetToken = null;
            recruiter.TokenExpiry = null;
            con.SaveChanges();

            TempData["Message"] = "Password has been reset successfully.";
            return RedirectToAction("Login");
        }
        private void SendEmail(string toEmail, string subject, string body)
        {
            string fromEmail = "sumayyakhanum.yasin@gmail.com";       // Replace with your Gmail
            string fromPassword = "jbyvoxtursrwglel";        // Gmail App Password

            using (var smtp = new SmtpClient("smtp.gmail.com", 587))
            {
                smtp.Credentials = new NetworkCredential(fromEmail, fromPassword);
                smtp.EnableSsl = true;

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(fromEmail);
                mail.To.Add(toEmail);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true; // Important for clickable link

                smtp.Send(mail);
            }
        }

    }
}
