using System;

namespace CareerMind.Domain.Entities
{
    public class JobApplication : BaseEntity
    {
        public Guid JobId { get; set; }
        public Job Job { get; set; } = null!;
        
        public Guid CandidateProfileId { get; set; }
        public CandidateProfile CandidateProfile { get; set; } = null!;
        
        public string Status { get; set; } = "Pending";
        public decimal? AiMatchScore { get; set; }
        public string? CoverLetter { get; set; }
    }
}
