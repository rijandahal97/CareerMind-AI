using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CareerMind.Application.DTOs.JobRisk;
using CareerMind.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CareerMind.API.Controllers
{
    /// <summary>
    /// Job Scam Risk Analysis endpoints.
    /// All endpoints require an authenticated JWT. Results are scoped to the requesting user.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class JobRiskController : ControllerBase
    {
        private readonly IJobRiskAnalysisService _riskService;
        private readonly ILogger<JobRiskController> _logger;

        public JobRiskController(
            IJobRiskAnalysisService riskService,
            ILogger<JobRiskController> logger)
        {
            _riskService = riskService;
            _logger = logger;
        }

        private Guid GetUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (idClaim == null || !Guid.TryParse(idClaim, out Guid id))
                throw new UnauthorizedAccessException("User ID is not found in token.");
            return id;
        }

        /// <summary>
        /// POST api/jobrisk/analyze
        /// Submit a job posting for scam risk analysis. The result is also persisted.
        /// </summary>
        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeJobRisk([FromBody] AnalyzeJobRiskRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _riskService.AnalyzeJobRiskAsync(GetUserId(), request);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing job risk analysis");
                return StatusCode(500, new { Message = "An error occurred while analysing the job posting." });
            }
        }

        /// <summary>
        /// GET api/jobrisk/history
        /// List all previous risk analyses performed by the current user, newest first.
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            try
            {
                var history = await _riskService.GetRiskAnalysisHistoryAsync(GetUserId());
                return Ok(history);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving job risk analysis history");
                return StatusCode(500, new { Message = "An error occurred while retrieving history." });
            }
        }

        /// <summary>
        /// GET api/jobrisk/{id}
        /// Retrieve a single previously-saved analysis. Returns 403 if it does not belong to the user.
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetAnalysis(Guid id)
        {
            try
            {
                var result = await _riskService.GetRiskAnalysisAsync(GetUserId(), id);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving job risk analysis {Id}", id);
                return StatusCode(500, new { Message = "An error occurred while retrieving the analysis." });
            }
        }
    }
}
