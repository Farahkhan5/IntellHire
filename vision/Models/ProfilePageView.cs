namespace vision.Models
{
    public class ProfilePageView
    {
        public required Profile User { get; set; }
        public List<AppliedCandidateViewModel> MyApplications { get; set; }

        public class Profile
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string? ProfileImagePath { get; set; }
        }
    }
}
