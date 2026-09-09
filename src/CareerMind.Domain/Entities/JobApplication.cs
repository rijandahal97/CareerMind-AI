using System;

namespace CareerMind.Domain.Entities
{
    public class JobApplication : BaseEntity
    {
        public Guid JobId { get; set; }
        public Job Job { get; set; } = null!;
        
        public Guid CandidateProfileId { get; set; }
        public CandidateProfile CandidateProfile { get; set; } = null!;
        
        public string Status { get; set; } = "Applied"; // Applied, UnderReview, Shortlisted, Interview, Rejected, Hired, Withdrawn
        public decimal? AiMatchScore { get; set; }
        public string? CoverLetter { get; set; }
        public string? ResumeUrl { get; set; }
        
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    }
}
