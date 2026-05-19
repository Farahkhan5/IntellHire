using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net.Mail;
using System.Net;
using vision.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Server;
using Newtonsoft.Json;
using System.Security.Claims;

using vision.Helpers; // EmailHelper ka namespace
namespace vision.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public HomeController(IWebHostEnvironment env)
        {
            _env = env;
        }

        Connection con = new Connection();
        private register GetCurrentUser()
        {
            var userId = HttpContext.Session.GetInt32("userId");
            if (userId == null)
                return null;

            var user = con.register.FirstOrDefault(u => u.Id == userId);
            return user;
        }
        public IActionResult Index(string category)
        {
             var jobs = con.jobs
                .Include(j => j.Company)
                .ToList();

            if (!string.IsNullOrEmpty(category))
            {
                jobs = jobs.Where(j => j.title == category).ToList();
            }

             var allowedTitles = new List<string>
    {
        "Marketing",
        "Customer Service",
        "Human Resource",
        "Project Management",
        "Business Development",
        "Sales & Communication",
        "Teaching & Education",
        "Design & Creative"
    };

            var categories = con.jobs
                .Where(j => allowedTitles.Contains(j.title))
                .GroupBy(j => j.title)
                .Select(g => new CategoryViewModel
                {
                    Name = g.Key,
                    VacancyCount = g.Count()
                })
                .ToList();

            var viewModel = new JobCategoryViewModel
            {
                Jobs = jobs,
                Categories = categories
            };

            return View(viewModel);
        }


        public IActionResult Job(string category)
        {
            var jobs = con.jobs
                          .Include(j => j.Company)
                          .ToList();

            if (!string.IsNullOrEmpty(category))
            {
                 jobs = jobs.Where(j => j.title == category).ToList();
            }

            var model = new JobCategoryViewModel
            {
                Jobs = jobs,
                Categories = new List<CategoryViewModel>() 
            };

            return View(model);
        }





        [HttpPost]
        public IActionResult SubscribeEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["Message"] = "Please enter a valid email address.";
                return RedirectToAction("Index");
            }

            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("sumayyakhanum.yasin@gmail.com", "jbyvoxtursrwglel"),
                    EnableSsl = true,
                };

                smtpClient.Send("sumayyakhanum.yasin@gmail.com", email,
                    "Subscription Confirmation",
                    "🎉 Congratulations! You have subscribed to IntellHire.");

                TempData["Message"] = "Subscription email sent successfully!";
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error sending email: " + ex.Message;
            }

            return RedirectToAction("Index");
        }


        public IActionResult About()
		{
			return View();
		}

		public IActionResult Job_list()
		{


            List<Job> data;
            try
            {
                data = con.jobs
                 .Include(j => j.Company) // include company
                 .ToList();
            }

            catch (Exception ex)
            {
                data = new List<Job>();
            }

            return View(data);
        }

        [HttpGet]
        public IActionResult Job_detail(int id)
        {
            var register = GetCurrentUser();
            if (register == null)
            {
                return RedirectToAction("Register", "Home");
            }
            //var job = con.jobs.FirstOrDefault(j => j.id == id);
            var job = con.jobs
        .Include(j => j.Company) // 👉 this line is essential
        .FirstOrDefault(j => j.id == id);

            if (job == null)
            {
                return NotFound();
            }

            var viewModel = new JobApplicationPageViewModel
            {
                JobDetail = job,
                Application = new ApplyJobViewModel
                {
                    JobId = job.id,
                }
            };

            return View("~/Views/Home/Job_detail.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Job_detail(JobApplicationPageViewModel model)
        {
            // ✅ Get current user
            var register = GetCurrentUser();
            if (register == null)
            {
                return RedirectToAction("Register", "Home");
            }

            // Remove unnecessary validation keys
            ModelState.Remove("JobDetail");
            ModelState.Remove("JobDetail.title");
            ModelState.Remove("JobDetail.email");
            ModelState.Remove("JobDetail.type");
            ModelState.Remove("JobDetail.address");
            ModelState.Remove("JobDetail.qualifications");
            ModelState.Remove("JobDetail.salary");
            ModelState.Remove("JobDetail.responsibility");
            ModelState.Remove("JobDetail.description");
            ModelState.Remove("JobTitle");

           



            // Ensure job detail is populated
            model.JobDetail = con.jobs.FirstOrDefault(j => j.id == model.Application.JobId);

            if (!ModelState.IsValid)
            {
                TempData["FormErrors"] = "Please fill the form fields.";
                var job = con.jobs.Include(j => j.Company)
                                  .FirstOrDefault(j => j.id == model.Application.JobId);
                model.JobDetail = job ?? new Job();
                return View(model);
            }
            // ✅ Duplicate check
            bool alreadyApplied = con.ApplyJobs
                .Any(a => a.JobId == model.Application.JobId &&
                          a.UserId == register.Id);

            if (alreadyApplied)
            {
                TempData["FormErrors"] = "You have already applied for this job.";
                return RedirectToAction("Job_detail", new { id = model.Application.JobId });
            }

            // ✅ Force email to be registered one
            model.Application.Email = register.Email;

            try
            {
                var file = model.Application.ResumeFile;
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/resumes");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                // ✅ Add UserId from logged-in user
                var application = new ApplyJob
                {
                    JobId = model.Application.JobId,
                    UserId = register.Id, // Important for FK constraint
                    Name = model.Application.Name,
                    Email = register.Email, // ✅ Email yahan set hogi
                    Qualification = model.Application.Qualification,
                    Message = model.Application.Message,
                    ResumeFilePath = "/uploads/resumes/" + uniqueFileName,
                    ApplicationStatus = "Pending",
                    AppliedAt = DateTime.Now
                };

                con.ApplyJobs.Add(application);
                con.SaveChanges();

                TempData["SuccessMessage"] = "Application submitted successfully!";
                return RedirectToAction("Job_detail", new { id = model.Application.JobId });
            }
            catch (Exception ex)
            {
                TempData["FormErrors"] = "Something went wrong: " + ex.Message;
                Console.WriteLine("Exception: " + ex);
                return View(model);
            }
        }




        //public IActionResult Job_detail()
        //{
        //	return View();
        //}

        public IActionResult Category()
		{
			return View();
		}

		public IActionResult Testimonial()
		{
			return View();
		}
        [HttpGet]
		public IActionResult Contact()
		{
			return View();
		}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(Contact model)
        {
            if (ModelState.IsValid)
            {
                con.ContactForms.Add(model);
                con.SaveChanges();

                TempData["SuccessMessage"] = "Your message has been submitted successfully!";
                return RedirectToAction("Contact"); // stay on Contact page
            }

            return View(model); // show validation errors if any
        }
        [HttpGet]
        public IActionResult Register()
        {

            return View();
        }

        [HttpPost]
        public IActionResult Register(register reg)
        {
            if (con.register.Any(x => x.Email == reg.Email))
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(reg);
            }

            if (ModelState.IsValid)
            {
                con.register.Add(reg);
                con.SaveChanges();

              

                TempData["msg"] = "Signup successful!";
                return RedirectToAction("Register");

            }
            return View();
        }

        [HttpPost]
        public IActionResult Login(register reg)
        {
            var user = con.register.FirstOrDefault(x => x.Email == reg.Email && x.Password == reg.Password);
            if (user != null)
            {
                HttpContext.Session.SetInt32("userId", user.Id);
                HttpContext.Session.SetString("Name", user.Name);

                // Convert Register -> Profile (DB ka data use karo)
                Profile profile = new Profile
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    ProfileImagePath = "", // Initially no image
                };

                HttpContext.Session.SetString("UserData", JsonConvert.SerializeObject(profile));

                return RedirectToAction("Index", "Home");
            }
            ViewBag.Error = "Invalid Email or Password";
            return View("Register");
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }

            var user = con.register.FirstOrDefault(x => x.ResetToken == token && x.TokenExpiry > DateTime.Now);
            if (user == null)
            {
                TempData["msg"] = "Invalid or expired reset token.";
                return RedirectToAction("ForgetPassword");
            }

            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(string token, string newPassword)
        {
            var user = con.register.FirstOrDefault(x => x.ResetToken == token && x.TokenExpiry > DateTime.Now);
            if (user == null)
            {
                TempData["msg"] = "Invalid or expired token.";
                return RedirectToAction("ForgetPassword");
            }

            user.Password = newPassword;
            user.ResetToken = null;
            user.TokenExpiry = null;
            con.SaveChanges();

            TempData["msg"] = "Password reset successful. Please login.";
            return RedirectToAction("Register");
        }


        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgetPassword(string Email)
        {
            var user = con.register.FirstOrDefault(x => x.Email == Email);

            if (user == null)
            {
                TempData["msg"] = "Email not found.";
                return View();
            }

            // Generate reset token
            user.ResetToken = Guid.NewGuid().ToString();
            user.TokenExpiry = DateTime.Now.AddHours(1);
            con.SaveChanges();

            // Create reset link
            var resetLink = Url.Action("ResetPassword", "Home", new { token = user.ResetToken }, Request.Scheme);

            // Send email
            var subject = "Reset Your Password";
            var body = $"Click here to reset your password: <a href='{resetLink}'>Reset Password</a>";

            SendEmail(user.Email, subject, body);

            TempData["msg"] = "Reset link sent to your email.";
            return View();
        }

        public void SendEmail(string toEmail, string subject, string body)
        {
            var fromEmail = "sumayyakhanum.yasin@gmail.com";
            var fromPassword = "jbyvoxtursrwglel";

            var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(fromEmail, fromPassword),
                EnableSsl = true
            };

            var mail = new MailMessage(fromEmail, toEmail, subject, body);
            mail.IsBodyHtml = true;
            smtp.Send(mail);
        }
        public IActionResult Registerout()
        {
            HttpContext.Session.Clear();
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public IActionResult Profile()
        {
            var userJson = HttpContext.Session.GetString("UserData");

            var user = new ProfilePageView.Profile();
            if (!string.IsNullOrEmpty(userJson))
            {
                user = JsonConvert.DeserializeObject<ProfilePageView.Profile>(userJson);
            }

            var myApps = (from app in con.ApplyJobs
                          join job in con.jobs on app.JobId equals job.id
                          join u in con.register on Convert.ToInt32(app.UserId) equals u.Id
                          where Convert.ToInt32(app.UserId) == user.Id
                          select new AppliedCandidateViewModel
                          {
                              Id = app.Id,
                              Name = u.Name,
                              JobTitle = job.title,
                              ProfileImagePath = u.ProfileImagePath,
                              ResumeFilePath = app.ResumeFilePath,
                              ApplicationStatus = app.ApplicationStatus
                          }).ToList();

            var vm = new ProfilePageView
            {
                User = user,
                MyApplications = myApps
            };

            return View(vm);
        }




        [HttpPost]
        public IActionResult UploadProfileImage(IFormFile ProfileImage)
        {
            // 1. Session se current user ID le lo
            int? userId = HttpContext.Session.GetInt32("userId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            if (ProfileImage != null && ProfileImage.Length > 0)
            {
                // 2. File ka path set karo
                var fileName = Path.GetFileName(ProfileImage.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                // 3. File ko server par save karo
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ProfileImage.CopyTo(stream);
                }

                // 4. Database me image path save karo
                var user = con.register.FirstOrDefault(u => u.Id == userId);
                if (user != null)
                {
                    user.ProfileImagePath = "/images/" + fileName; // relative path
                    con.SaveChanges();

                    // 5. Session me bhi update karo
                    var profile = new Profile
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Email = user.Email,
                        ProfileImagePath = user.ProfileImagePath
                    };
                    HttpContext.Session.SetString("UserData", JsonConvert.SerializeObject(profile));
                }
            }

            return RedirectToAction("Profile");
        }



    }
}
