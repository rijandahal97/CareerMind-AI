using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CareerMind.Application.Interfaces;
using CareerMind.Application.DTOs.Candidate;

namespace CareerMind.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CandidatesController : ControllerBase
    {
        private readonly ICandidateProfileService _profileService;
        private readonly IJobService _jobService;
        private readonly ILogger<CandidatesController> _logger;

        public CandidatesController(ICandidateProfileService profileService, IJobService jobService, ILogger<CandidatesController> logger)
        {
            _profileService = profileService;
            _jobService = jobService;
            _logger = logger;
        }

        private Guid GetUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (idClaim == null || !Guid.TryParse(idClaim, out Guid id))
            {
                throw new UnauthorizedAccessException("User ID is not found in token.");
            }
            return id;
        }

        [HttpGet("me/profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = GetUserId();
                var profile = await _profileService.GetProfileAsync(userId);
                if (profile == null) return NotFound("Profile not found.");
                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting profile");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("me/profile")]
        [HttpPut("me/profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateCandidateProfileDto dto)
        {
            try
            {
                var userId = GetUserId();
                var updated = await _profileService.UpdateProfileAsync(userId, dto);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("me/skills")]
        public async Task<IActionResult> GetSkills()
        {
            var userId = GetUserId();
            var profile = await _profileService.GetProfileAsync(userId);
            return profile == null ? NotFound() : Ok(profile.Skills);
        }

        [HttpPost("me/skills")]
        public async Task<IActionResult> AddSkill([FromBody] AddCandidateSkillDto dto)
        {
            var userId = GetUserId();
            var result = await _profileService.AddSkillAsync(userId, dto);
            return Ok(result);
        }
        
        [HttpPut("me/skills/{id}")]
        public async Task<IActionResult> UpdateSkill(Guid id, [FromBody] UpdateCandidateSkillDto dto)
        {
            var userId = GetUserId();
            var result = await _profileService.UpdateSkillAsync(userId, id, dto);
            return Ok(result);
        }

        [HttpDelete("me/skills/{id}")]
        public async Task<IActionResult> RemoveSkill(Guid id)
        {
            var userId = GetUserId();
            var result = await _profileService.RemoveSkillAsync(userId, id);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpGet("me/education")]
        public async Task<IActionResult> GetEducations()
        {
            var userId = GetUserId();
            var profile = await _profileService.GetProfileAsync(userId);
            return profile == null ? NotFound() : Ok(profile.Educations);
        }

        [HttpPost("me/education")]
        public async Task<IActionResult> AddEducation([FromBody] AddEducationDto dto)
        {
            var userId = GetUserId();
            var result = await _profileService.AddEducationAsync(userId, dto);
            return Ok(result);
        }

        [HttpPut("me/education/{id}")]
        public async Task<IActionResult> UpdateEducation(Guid id, [FromBody] UpdateEducationDto dto)
        {
            var userId = GetUserId();
            var result = await _profileService.UpdateEducationAsync(userId, id, dto);
            return Ok(result);
        }

        [HttpDelete("me/education/{id}")]
        public async Task<IActionResult> RemoveEducation(Guid id)
        {
            var userId = GetUserId();
            var result = await _profileService.RemoveEducationAsync(userId, id);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpGet("me/experience")]
        public async Task<IActionResult> GetExperiences()
        {
            var userId = GetUserId();
            var profile = await _profileService.GetProfileAsync(userId);
            return profile == null ? NotFound() : Ok(profile.WorkExperiences);
        }

        [HttpPost("me/experience")]
        public async Task<IActionResult> AddExperience([FromBody] AddWorkExperienceDto dto)
        {
            var userId = GetUserId();
            // Basic validation
            if (dto.IsCurrent) dto.EndDate = null;
            if (dto.EndDate.HasValue && dto.StartDate > dto.EndDate.Value) return BadRequest("Start date must be before end date.");
            
            var result = await _profileService.AddExperienceAsync(userId, dto);
            return Ok(result);
        }

        [HttpPut("me/experience/{id}")]
        public async Task<IActionResult> UpdateExperience(Guid id, [FromBody] UpdateWorkExperienceDto dto)
        {
            var userId = GetUserId();
            if (dto.IsCurrent) dto.EndDate = null;
            if (dto.EndDate.HasValue && dto.StartDate > dto.EndDate.Value) return BadRequest("Start date must be before end date.");
            
            var result = await _profileService.UpdateExperienceAsync(userId, id, dto);
            return Ok(result);
        }

        [HttpDelete("me/experience/{id}")]
        public async Task<IActionResult> RemoveExperience(Guid id)
        {
            var userId = GetUserId();
            var result = await _profileService.RemoveExperienceAsync(userId, id);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpGet("me/certifications")]
        public async Task<IActionResult> GetCertifications()
        {
            var userId = GetUserId();
            var profile = await _profileService.GetProfileAsync(userId);
            return profile == null ? NotFound() : Ok(profile.Certifications);
        }

        [HttpPost("me/certifications")]
        public async Task<IActionResult> AddCertification([FromBody] AddCertificationDto dto)
        {
            var userId = GetUserId();
            var result = await _profileService.AddCertificationAsync(userId, dto);
            return Ok(result);
        }

        [HttpPut("me/certifications/{id}")]
        public async Task<IActionResult> UpdateCertification(Guid id, [FromBody] UpdateCertificationDto dto)
        {
            var userId = GetUserId();
            var result = await _profileService.UpdateCertificationAsync(userId, id, dto);
            return Ok(result);
        }

        [HttpDelete("me/certifications/{id}")]
        public async Task<IActionResult> RemoveCertification(Guid id)
        {
            var userId = GetUserId();
            var result = await _profileService.RemoveCertificationAsync(userId, id);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpGet("me/languages")]
        public async Task<IActionResult> GetLanguages()
        {
            var userId = GetUserId();
            var profile = await _profileService.GetProfileAsync(userId);
            return profile == null ? NotFound() : Ok(profile.Languages);
        }

        [HttpPost("me/languages")]
        public async Task<IActionResult> AddLanguage([FromBody] AddCandidateLanguageDto dto)
        {
            var userId = GetUserId();
            var result = await _profileService.AddLanguageAsync(userId, dto);
            return Ok(result);
        }

        [HttpPut("me/languages/{id}")]
        public async Task<IActionResult> UpdateLanguage(Guid id, [FromBody] UpdateCandidateLanguageDto dto)
        {
            var userId = GetUserId();
            var result = await _profileService.UpdateLanguageAsync(userId, id, dto);
            return Ok(result);
        }

        [HttpDelete("me/languages/{id}")]
        public async Task<IActionResult> RemoveLanguage(Guid id)
        {
            var userId = GetUserId();
            var result = await _profileService.RemoveLanguageAsync(userId, id);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpGet("me/preferences")]
        public async Task<IActionResult> GetPreferences()
        {
            var userId = GetUserId();
            var pref = await _profileService.GetPreferencesAsync(userId);
            return pref == null ? NotFound() : Ok(pref);
        }

        [HttpPut("me/preferences")]
        public async Task<IActionResult> UpdatePreferences([FromBody] UpdateCareerPreferenceDto dto)
        {
            var userId = GetUserId();
            var result = await _profileService.UpdatePreferencesAsync(userId, dto);
            return Ok(result);
        }

        // Search options
        [HttpGet("available-skills")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableSkills([FromQuery] string query = "")
        {
            var skills = await _profileService.GetAvailableSkillsAsync(query);
            return Ok(skills);
        }

        [HttpGet("me/applications")]
        public async Task<IActionResult> GetMyApplications()
        {
            try
            {
                var apps = await _jobService.GetCandidateApplicationsAsync(GetUserId());
                return Ok(apps);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("me/saved-jobs")]
        public async Task<IActionResult> GetMySavedJobs()
        {
            try
            {
                var saved = await _jobService.GetSavedJobsAsync(GetUserId());
                return Ok(saved);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
