namespace vision.Models
{
	public class AppliedCandidateViewModel
	{
		public int Id { get; set; }
		public string JobTitle { get; set; }
		public string Name { get; set; }
		public string ResumeFilePath { get; set; }
        public string ProfileImagePath { get; set; }
		public string ApplicationStatus { get; set; } // Interview, Pending, Rejected, etc.
	}
}
