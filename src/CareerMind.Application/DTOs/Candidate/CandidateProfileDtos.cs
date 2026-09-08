using System;
using CareerMind.Domain.Enums;
using System.Collections.Generic;

namespace CareerMind.Application.DTOs.Candidate
{
    public class CandidateProfileDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? Headline { get; set; }
        public string? Bio { get; set; }
        public string? CareerSummary { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? ResumeUrl { get; set; }
        public string? CurrentJobTitle { get; set; }
        public string? CurrentCompany { get; set; }
        public int? YearsOfExperience { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? GitHubUrl { get; set; }
        public string? PortfolioUrl { get; set; }
        public string? AvailabilityStatus { get; set; }
        public int CompletionPercentage { get; set; }
        
        public ICollection<CandidateSkillDto> Skills { get; set; } = new List<CandidateSkillDto>();
        public ICollection<EducationDto> Educations { get; set; } = new List<EducationDto>();
        public ICollection<WorkExperienceDto> WorkExperiences { get; set; } = new List<WorkExperienceDto>();
        public ICollection<CertificationDto> Certifications { get; set; } = new List<CertificationDto>();
        public ICollection<CandidateLanguageDto> Languages { get; set; } = new List<CandidateLanguageDto>();
        public CareerPreferenceDto? CareerPreference { get; set; }
    }

    public class UpdateCandidateProfileDto
    {
        public string? Headline { get; set; }
        public string? Bio { get; set; }
        public string? CareerSummary { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? CurrentJobTitle { get; set; }
        public string? CurrentCompany { get; set; }
        public int? YearsOfExperience { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? GitHubUrl { get; set; }
        public string? PortfolioUrl { get; set; }
        public string? AvailabilityStatus { get; set; }
    }
}
