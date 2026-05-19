using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vision.Models;

namespace vision.Controllers
{
    public class Admin : Controller
    {
        Connection con = new Connection();

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            string adminEmail = "emily@gmail.com";
            string adminPassword = "admin@12345";

            if (email == adminEmail && password == adminPassword)
            {
                HttpContext.Session.SetString("IsAdmin", "true");
                HttpContext.Session.SetString("AdminEmail", email); // Save email in session
                return RedirectToAction("Index", "Admin");
            }
            else
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }
        }

        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
            {
                return RedirectToAction("Login", "Admin");
            }

            var model = new DashboardViewModel
            {
                RegisteredUsersCount = con.register.Count(),  // unique users count
                VisitorsCount = con.signup.Count(),
                RegisteredUsers = con.register.ToList(),
                recruitersignup = con.signup.ToList(),
            };

            return View(model);
        }
        public IActionResult User()
        {
            var model = new DashboardViewModel
            {

                RegisteredUsers = con.register.ToList(),


            };

            return View(model);
        }
        public IActionResult Recuriter()
        {
            var model = new DashboardViewModel
            {

                recruitersignup = con.signup.ToList(),


            };

            return View(model);
        }
        [HttpPost]
        public IActionResult DeleteUser(int id)
        {
            // Find user by ID
            var user = con.register.FirstOrDefault(u => u.Id == id);

            if (user != null)
            {
                con.register.Remove(user); // Remove user from DB
                con.SaveChanges(); // Commit changes
                TempData["SuccessMessage"] = "User deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "User not found.";
            }

            // Redirect back to dashboard or user list page
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DeleteRecruiter(int id)
        {
            var recruiter = con.signup.FirstOrDefault(r => r.Id == id);

            if (recruiter != null)
            {
                // Pehle related jobs delete karo
                var relatedJobs = con.jobs.Where(j => j.CompanyId == recruiter.Id).ToList();
                con.jobs.RemoveRange(relatedJobs);

                con.signup.Remove(recruiter);
                con.SaveChanges();

                TempData["SuccessMessage"] = "Recruiter and related jobs deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Recruiter not found.";
            }

            return RedirectToAction("Index");
        }

        public IActionResult Payments()
        {
            var payments = con.Payments
         .Include(p => p.Recruiter)
         .ToList();

            return View(payments); // ✅ Return List<Payment>

        }
        [HttpPost]
        public IActionResult DeletePayment(int id)
        {
            // Find the payment by its ID
            var payment = con.Payments.FirstOrDefault(p => p.Id == id);

            if (payment != null)
            {
                con.Payments.Remove(payment);
                con.SaveChanges();
                TempData["SuccessMessages"] = "Payment deleted successfully.";
            }
            else
            {
                TempData["ErrorMessages"] = "Payment not found.";
            }

            return RedirectToAction("Payments");
        }


    }
}
