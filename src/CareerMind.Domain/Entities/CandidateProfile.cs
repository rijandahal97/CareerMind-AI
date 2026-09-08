using System;
using System.Collections.Generic;

namespace CareerMind.Domain.Entities
{
    public class CandidateProfile : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        
        public string? Headline { get; set; }
        public string? Bio { get; set; }
        public string? CareerSummary { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? ResumeUrl { get; set; } // preexisting

        public string? CurrentJobTitle { get; set; }
        public string? CurrentCompany { get; set; }
        public int? YearsOfExperience { get; set; }
        
        // Links
        public string? LinkedInUrl { get; set; }
        public string? GitHubUrl { get; set; }
        public string? PortfolioUrl { get; set; }
        
        public string? AvailabilityStatus { get; set; }

        public ICollection<CandidateSkill> CandidateSkills { get; set; } = new List<CandidateSkill>();
        public ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
        
        public ICollection<Education> Educations { get; set; } = new List<Education>();
        public ICollection<WorkExperience> WorkExperiences { get; set; } = new List<WorkExperience>();
        public ICollection<Certification> Certifications { get; set; } = new List<Certification>();
        public ICollection<CandidateLanguage> CandidateLanguages { get; set; } = new List<CandidateLanguage>();
        
        public CareerPreference? CareerPreference { get; set; }
    }
}
