namespace vision.Models
{
    public class JobCategoryViewModel
    {
        public IEnumerable<Job> Jobs { get; set; }
        public IEnumerable<CategoryViewModel> Categories { get; set; }
    }
}
