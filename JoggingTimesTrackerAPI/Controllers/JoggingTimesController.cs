using JoggingTimesTrackerDAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;
using System.Security.Claims;

namespace JoggingTimesTrackerAPI.Controllers
{
    [Authorize(Roles = "Admin,User")]
    [Route("api/[controller]")]
    [ApiController]
    public class JoggingTimesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public JoggingTimesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<IEnumerable<JoggingTime>>> GetJoggingTimes(DateTime? fromDate, DateTime? toDate)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("Invalid user");
            }

            var joggingTimes = _context.JoggingTimes
                .Where(j => j.UserId == userId);

            if (fromDate != null)
            {
                joggingTimes = joggingTimes.Where(j => j.Date >= fromDate.Value.Date);
            }

            if (toDate != null)
            {
                joggingTimes = joggingTimes.Where(j => j.Date <= toDate.Value.Date);
            }

            return await joggingTimes.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<JoggingTime>> GetJoggingTime(int id)
        {
            var joggingTime = await _context.JoggingTimes.FindAsync(id);

            if (joggingTime == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(joggingTime.UserId);

            if (user == null)
            {
                return BadRequest("Invalid user");
            }

            if (User.FindFirst(ClaimTypes.Role)?.Value != "Admin" && user.Id != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
            {
                return Forbid();
            }

            return joggingTime;
        }

        [HttpPost]
        public async Task<ActionResult<JoggingTime>> PostJoggingTime(JoggingTime joggingTime)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("Invalid user");
            }

            joggingTime.UserId = userId;

            _context.JoggingTimes.Add(joggingTime);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetJoggingTime), new { id = joggingTime.Id }, joggingTime);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutJoggingTime(int id, JoggingTime joggingTime)
        {
            if (id != joggingTime.Id)
            {
                return BadRequest();
            }

            var existingJoggingTime = await _context.JoggingTimes.FindAsync(id);

            if (existingJoggingTime == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(existingJoggingTime.UserId);

            if (user == null)
            {
                return BadRequest("Invalid user");
            }
            if (User.FindFirst(ClaimTypes.Role)?.Value != "Admin" && user.Id != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
            {
                return Forbid();
            }

            existingJoggingTime.Date = joggingTime.Date;
            existingJoggingTime.Distance = joggingTime.Distance;
            existingJoggingTime.Time = joggingTime.Time;

            _context.Entry(existingJoggingTime).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<JoggingTime>> DeleteJoggingTime(int id)
        {
            var joggingTime = await _context.JoggingTimes.FindAsync(id);

            if (joggingTime == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(joggingTime.UserId);

            if (user == null)
            {
                return BadRequest("Invalid user");
            }

            if (User.FindFirst(ClaimTypes.Role)?.Value != "Admin" && user.Id != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
            {
                return Forbid();
            }

            _context.JoggingTimes.Remove(joggingTime);
            await _context.SaveChangesAsync();

            return joggingTime;
        }

        [HttpGet("report")]
        public async Task<ActionResult<IEnumerable<object>>> GetJoggingTimeReport(DateTime? fromDate, DateTime? toDate)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("Invalid user");
            }

            var joggingTimes = _context.JoggingTimes
                .Where(j => j.UserId == userId);

            if (fromDate != null)
            {
                joggingTimes = joggingTimes.Where(j => j.Date >= fromDate.Value.Date);
            }

            if (toDate != null)
            {
                joggingTimes = joggingTimes.Where(j => j.Date <= toDate.Value.Date);
            }

            var report = joggingTimes
                .GroupBy(j => new { Year = j.Date.Year, Week = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(j.Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday) })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Week = g.Key.Week,
                    AverageSpeed = g.Sum(j => j.Distance) / g.Sum(j => j.Time),
                    AverageDistance = g.Average(j => j.Distance)
                });

            return await report.ToListAsync();
        }
    }
    }
