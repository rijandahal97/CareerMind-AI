using System;
using System.ComponentModel.DataAnnotations;

namespace CareerMind.Application.DTOs.Job
{
    public class JobApplicationDto
    {
        public Guid Id { get; set; }
        public Guid JobId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        
        public Guid CandidateProfileId { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        
        public string Status { get; set; } = string.Empty;
        public decimal? AiMatchScore { get; set; }
        
        public string? CoverLetter { get; set; }
        public string? ResumeUrl { get; set; }
        
        public DateTime AppliedAt { get; set; }
    }

    public class ApplyJobRequest
    {
        public string? CoverLetter { get; set; }
        [Url]
        public string? ResumeUrl { get; set; }
    }

    public class UpdateApplicationStatusRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty; // e.g. UnderReview, Shortlisted, Rejected, etc.
    }
}
