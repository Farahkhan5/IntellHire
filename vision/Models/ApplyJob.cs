namespace vision.Models
{
    public class ApplyJob
    {
        public int Id { get; set; }

        public int JobId { get; set; }

        public int UserId { get; set; } // <-- Add this field
        public string Name { get; set; }
      
        public string Email { get; set; }


        public string Qualification { get; set; }

        public string Message { get; set; }

        public string ResumeFilePath { get; set; }

		public string ApplicationStatus { get; set; }

		public DateTime AppliedAt { get; set; }
    }

}
