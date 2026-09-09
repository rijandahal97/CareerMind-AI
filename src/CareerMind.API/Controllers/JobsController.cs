using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CareerMind.Application.DTOs.Job;
using CareerMind.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareerMind.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly IJobService _jobService;

        public JobsController(IJobService jobService)
        {
            _jobService = jobService;
        }

        private Guid GetUserId()
        {
            return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        }

        [HttpGet]
        public async Task<IActionResult> SearchJobs([FromQuery] string? keyword, [FromQuery] Guid? categoryId, [FromQuery] string? location, [FromQuery] bool? isRemote, [FromQuery] string? employmentType, [FromQuery] string? experienceLevel)
        {
            var jobs = await _jobService.SearchJobsAsync(keyword, categoryId, location, isRemote, employmentType, experienceLevel);
            return Ok(jobs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetJobDetails(Guid id)
        {
            try
            {
                var job = await _jobService.GetJobDetailsAsync(id);
                return Ok(job);
            }
            catch (Exception ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        [HttpPost("{id}/apply")]
        [Authorize(Roles = "Candidate")]
        public async Task<IActionResult> ApplyForJob(Guid id, ApplyJobRequest request)
        {
            try
            {
                var app = await _jobService.ApplyForJobAsync(GetUserId(), id, request);
                return Ok(app);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("{id}/save")]
        [Authorize(Roles = "Candidate")]
        public async Task<IActionResult> SaveJob(Guid id)
        {
            try
            {
                await _jobService.SaveJobAsync(GetUserId(), id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpDelete("{id}/save")]
        [Authorize(Roles = "Candidate")]
        public async Task<IActionResult> RemoveSavedJob(Guid id)
        {
            try
            {
                await _jobService.RemoveSavedJobAsync(GetUserId(), id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
