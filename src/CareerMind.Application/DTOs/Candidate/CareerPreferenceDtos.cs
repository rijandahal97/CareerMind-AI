using System;

namespace CareerMind.Application.DTOs.Candidate
{
    public class CareerPreferenceDto
    {
        public Guid Id { get; set; }
        public string? PreferredJobRoles { get; set; }
        public string? PreferredIndustries { get; set; }
        public string? PreferredLocations { get; set; }
        public bool OpenToRemote { get; set; }
        public bool WillingToRelocate { get; set; }
        public string? PreferredEmploymentTypes { get; set; }
        public decimal? PreferredSalaryMin { get; set; }
        public decimal? PreferredSalaryMax { get; set; }
        public string? CareerInterests { get; set; }
    }

    public class UpdateCareerPreferenceDto
    {
        public string? PreferredJobRoles { get; set; }
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
