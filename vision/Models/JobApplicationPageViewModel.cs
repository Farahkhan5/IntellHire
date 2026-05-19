namespace vision.Models
{
    public class JobApplicationPageViewModel
    {
			public JobApplicationPageViewModel()
			{
				JobDetail = new Job();
			}
			public Job JobDetail { get; set; }

			public string JobTitle { get; set; }

			public ApplyJobViewModel Application { get; set; }
		}
	}
