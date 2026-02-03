using Microsoft.EntityFrameworkCore;
using job_tracker_api.Models;

namespace job_tracker_api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<JobApplication> JobApplications { get; set; }
    }
}