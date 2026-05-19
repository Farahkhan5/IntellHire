using vision.Controllers;

namespace vision.Models
{
    public class DashboardViewModel
    {
        public int RegisteredUsersCount { get; set; }
        public int VisitorsCount { get; set; }

        public List<register> RegisteredUsers { get; set; }

        public List<recruitersignup> recruitersignup { get; set; }

        public List<Payment> payments { get; set; }

    }
}
