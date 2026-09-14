using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using CareerMind.Application.DTOs.Resume;
using CareerMind.Application.Interfaces;

namespace CareerMind.API.Controllers
{
    [ApiController]
    [Route("api/candidates/me/resumes")]
    [Authorize]
    public class ResumeController : ControllerBase
    {
        private readonly IResumeIntelligenceService _resumeService;
        private readonly ILogger<ResumeController> _logger;

        public ResumeController(IResumeIntelligenceService resumeService, ILogger<ResumeController> logger)
        {
            _resumeService = resumeService;
            _logger = logger;
        }

        private Guid GetUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (idClaim == null || !Guid.TryParse(idClaim, out Guid id))
                throw new UnauthorizedAccessException("User ID is not found in token.");
            return id;
        }

        // POST /api/candidates/me/resumes
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadResume([FromForm] Microsoft.AspNetCore.Http.IFormFile file)
        {
            try
            {
                var userId = GetUserId();
                var request = new ResumeUploadRequest
                {
                    OriginalFileName = file.FileName,
                    ContentType = file.ContentType,
                    Length = file.Length,
                    Content = file.OpenReadStream()
                };
                var dto = await _resumeService.UploadResumeAsync(userId, request);
                return CreatedAtAction(nameof(GetResume), new { id = dto.Id }, dto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading resume");
                return StatusCode(500, new { Message = "An error occurred uploading your resume." });
            }
        }

        // GET /api/candidates/me/resumes
        [HttpGet]
        public async Task<IActionResult> GetResumes()
        {
            try
            {
                var userId = GetUserId();
                var result = await _resumeService.GetResumesAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting resumes");
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        // GET /api/candidates/me/resumes/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetResume(Guid id)
        {
            try
            {
                var userId = GetUserId();
                var dto = await _resumeService.GetResumeAsync(userId, id);
                return Ok(dto);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting resume");
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        // DELETE /api/candidates/me/resumes/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteResume(Guid id)
        {
            try
            {
                var userId = GetUserId();
                await _resumeService.DeleteResumeAsync(userId, id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting resume");
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        // PUT /api/candidates/me/resumes/{id}/primary
        [HttpPut("{id:guid}/primary")]
        public async Task<IActionResult> SetPrimary(Guid id)
        {
            try
            {
                var userId = GetUserId();
                await _resumeService.SetPrimaryAsync(userId, id);
                return Ok(new { Message = "Primary resume updated." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting primary resume");
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        // POST /api/candidates/me/resumes/{resumeId}/analyze
        [HttpPost("{resumeId:guid}/analyze")]
        public async Task<IActionResult> Analyze(Guid resumeId)
        {
            try
            {
                var userId = GetUserId();
                var result = await _resumeService.AnalyzeResumeAsync(userId, resumeId, null);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing resume");
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        // POST /api/candidates/me/resumes/{resumeId}/analyze/{jobId}
        [HttpPost("{resumeId:guid}/analyze/{jobId:guid}")]
        public async Task<IActionResult> AnalyzeForJob(Guid resumeId, Guid jobId)
        {
            try
            {
                var userId = GetUserId();
                var result = await _resumeService.AnalyzeResumeAsync(userId, resumeId, jobId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing resume for job");
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        // GET /api/candidates/me/resumes/{resumeId}/analyses
        [HttpGet("{resumeId:guid}/analyses")]
        public async Task<IActionResult> GetAnalyses(Guid resumeId)
        {
            try
            {
                var userId = GetUserId();
                var result = await _resumeService.GetAnalysesAsync(userId, resumeId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting analyses");
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        // GET /api/candidates/me/resumes/{resumeId}/analyses/latest
        [HttpGet("{resumeId:guid}/analyses/latest")]
        public async Task<IActionResult> GetLatestAnalysis(Guid resumeId)
        {
            try
            {
                var userId = GetUserId();
                var result = await _resumeService.GetLatestAnalysisAsync(userId, resumeId);
                if (result == null) return NotFound(new { Message = "No analysis found for this resume." });
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest analysis");
                return StatusCode(500, new { Message = ex.Message });
            }
        }
    }
}
