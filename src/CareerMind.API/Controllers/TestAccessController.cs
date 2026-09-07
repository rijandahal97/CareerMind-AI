using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareerMind.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class TestAccessController : ControllerBase
    {
        [HttpGet("candidates/test-access")]
        [Authorize(Roles = "Candidate")]
        public IActionResult CandidateAccess()
        {
            return Ok(new { Message = "Candidate access granted!" });
        }

        [HttpGet("employers/test-access")]
        [Authorize(Roles = "Employer")]
        public IActionResult EmployerAccess()
        {
            return Ok(new { Message = "Employer access granted!" });
        }

        [HttpGet("admin/test-access")]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminAccess()
        {
            return Ok(new { Message = "Admin access granted!" });
        }
    }
}
