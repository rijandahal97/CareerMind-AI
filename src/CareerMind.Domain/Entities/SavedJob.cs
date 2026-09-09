using System;

namespace CareerMind.Domain.Entities
{
    public class SavedJob
    {
        public Guid CandidateProfileId { get; set; }
        public CandidateProfile CandidateProfile { get; set; } = null!;
        
        public Guid JobId { get; set; }
        public Job Job { get; set; } = null!;
        
        public DateTime SavedAt { get; set; } = DateTime.UtcNow;
    }
}
