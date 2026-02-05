using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using job_tracker_api.Data;
using job_tracker_api.Models;

namespace job_tracker_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobApplicationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public JobApplicationsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/jobapplications
        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobApplication>>> GetJobApplications([FromQuery] string? status)
        {
            if (string.IsNullOrEmpty(status))
            {
                // No filter - return all
                return await _context.JobApplications.ToListAsync();
            }
            
            // Filter by status
            return await _context.JobApplications
                .Where(j => j.Status == status)
                .ToListAsync();
        }


        // GET: api/jobapplications/5
        [HttpGet("{id}")]
        public async Task<ActionResult<JobApplication>> GetJobApplication(int id)
        {
            var jobApplication = await _context.JobApplications.FindAsync(id);

            if (jobApplication == null)
            {
                return NotFound();
            }

            return jobApplication;
        }

        // POST: api/jobapplications
        [HttpPost]
        public async Task<ActionResult<JobApplication>> CreateJobApplication(JobApplication jobApplication)
        {
            _context.JobApplications.Add(jobApplication);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetJobApplication), new { id = jobApplication.Id }, jobApplication);
        }

        // PUT: api/jobapplications/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateJobApplication(int id, JobApplication jobApplication)
        {
            if (id != jobApplication.Id)
            {
                return BadRequest();
            }

            jobApplication.UpdatedAt = DateTime.UtcNow;
            _context.Entry(jobApplication).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!JobApplicationExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/jobapplications/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJobApplication(int id)
        {
            var jobApplication = await _context.JobApplications.FindAsync(id);
            if (jobApplication == null)
            {
                return NotFound();
            }

            _context.JobApplications.Remove(jobApplication);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("weekly-progress")]
        public async Task<ActionResult<object>> GetWeeklyProgress()
        {
            // Get start of current week (Monday)
            var today = DateTime.UtcNow.Date;
            var daysSinceMonday = (int)today.DayOfWeek - (int)DayOfWeek.Monday;
            if (daysSinceMonday < 0) daysSinceMonday += 7; // Handle Sunday
            
            var weekStart = today.AddDays(-daysSinceMonday);
            var weekEnd = weekStart.AddDays(7);
            
            // Count applications added this week
            var applicationsThisWeek = await _context.JobApplications
                .Where(j => j.CreatedAt >= weekStart && j.CreatedAt < weekEnd)
                .CountAsync();
            
            var goal = 2;
            var isComplete = applicationsThisWeek >= goal;
            
            return Ok(new
            {
                applicationsThisWeek,
                goal,
                isComplete,
                weekStart = weekStart.ToString("yyyy-MM-dd"),
                weekEnd = weekEnd.AddDays(-1).ToString("yyyy-MM-dd"),
                remaining = Math.Max(0, goal - applicationsThisWeek)
            });
        }

        private bool JobApplicationExists(int id)
        {
            return _context.JobApplications.Any(e => e.Id == id);
        }
    }
}