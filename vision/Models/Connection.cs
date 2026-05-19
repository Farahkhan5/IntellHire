using Microsoft.EntityFrameworkCore;

namespace vision.Models
{
	public class Connection : DbContext
	{
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			base.OnConfiguring(optionsBuilder);

            optionsBuilder.UseSqlServer("Server=DESKTOP-HSDFA7G\\SQLEXPRESS;Integrated Security=true;Database=vision1;TrustServerCertificate=true;");

        }

        public DbSet<Company> Company { get; set; }
        public DbSet<ApplyJob> ApplyJobs { get; set; }
        public DbSet<Job> jobs { get; set; }
        public DbSet<register> register { get; set; }
        public DbSet<recruitersignup> signup { get; set; }

        public DbSet<Contact> ContactForms { get; set; }

        public DbSet<Payment> Payments { get; set; }

    }
}
