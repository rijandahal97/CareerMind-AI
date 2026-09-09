using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CareerMind.Application.DTOs.Employer;
using CareerMind.Application.DTOs.Job;
using CareerMind.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareerMind.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Employer")]
    public class EmployersController : ControllerBase
    {
        private readonly IEmployerProfileService _employerService;
        private readonly IJobService _jobService;

        public EmployersController(IEmployerProfileService employerService, IJobService jobService)
        {
            _employerService = employerService;
            _jobService = jobService;
        }

        private Guid GetUserId()
        {
            return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var profile = await _employerService.GetProfileAsync(GetUserId());
                return Ok(profile);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateProfile(UpdateEmployerProfileRequest request)
        {
            try
            {
                var profile = await _employerService.UpdateProfileAsync(GetUserId(), request);
                return Ok(profile);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("me/jobs")]
        public async Task<IActionResult> CreateJob(CreateJobRequest request)
        {
            try
            {
                var job = await _jobService.CreateJobAsync(GetUserId(), request);
                return CreatedAtAction(nameof(GetJob), new { id = job.Id }, job);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("me/jobs")]
        public async Task<IActionResult> GetJobs()
        {
            var jobs = await _jobService.GetEmployerJobsAsync(GetUserId());
            return Ok(jobs);
        }

        [HttpGet("me/jobs/{id}")]
        public async Task<IActionResult> GetJob(Guid id)
        {
            try
            {
                var job = await _jobService.GetEmployerJobByIdAsync(GetUserId(), id);
                return Ok(job);
            }
            catch (Exception ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        [HttpPut("me/jobs/{id}")]
        public async Task<IActionResult> UpdateJob(Guid id, UpdateJobRequest request)
        {
            try
            {
                var job = await _jobService.UpdateJobAsync(GetUserId(), id, request);
                return Ok(job);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpDelete("me/jobs/{id}")]
        public async Task<IActionResult> DeleteJob(Guid id)
        {
            try
            {
                await _jobService.DeleteJobAsync(GetUserId(), id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("me/jobs/{jobId}/applications")]
        public async Task<IActionResult> GetJobApplications(Guid jobId)
        {
            try
            {
                var apps = await _jobService.GetJobApplicationsAsync(GetUserId(), jobId);
                return Ok(apps);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("me/applications/{applicationId}/status")]
        public async Task<IActionResult> UpdateApplicationStatus(Guid applicationId, UpdateApplicationStatusRequest request)
        {
            try
            {
                await _jobService.UpdateApplicationStatusAsync(GetUserId(), applicationId, request);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
