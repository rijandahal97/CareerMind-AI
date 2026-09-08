using System;

namespace CareerMind.Domain.Entities
{
    public class CareerPreference : BaseEntity
    {
        public Guid CandidateProfileId { get; set; }
        public CandidateProfile CandidateProfile { get; set; } = null!;
        
        public string? PreferredJobRoles { get; set; } // Can store comma-separated or JSON
        public string? PreferredIndustries { get; set; }
        public string? PreferredLocations { get; set; }
        
        public bool OpenToRemote { get; set; }
        public bool WillingToRelocate { get; set; }
        
        public string? PreferredEmploymentTypes { get; set; }
        
        public decimal? PreferredSalaryMin { get; set; }
        public decimal? PreferredSalaryMax { get; set; }
        public string? CareerInterests { get; set; }
    }
}
