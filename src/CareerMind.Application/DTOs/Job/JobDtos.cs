using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CareerMind.Application.DTOs.Candidate; // For SkillDto if needed

namespace CareerMind.Application.DTOs.Job
{
    public class JobDto
    {
        public Guid Id { get; set; }
        public Guid EmployerProfileId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyLogoUrl { get; set; }
        
        public Guid JobCategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Responsibilities { get; set; }
        public string? Requirements { get; set; }
        
        public string? EmploymentType { get; set; }
        public string? WorkMode { get; set; }
        public string? ExperienceLevel { get; set; }
        
        public int? MinimumExperienceYears { get; set; }
        public int? MaximumExperienceYears { get; set; }
        
        public decimal? MinimumSalary { get; set; }
        public decimal? MaximumSalary { get; set; }
        public string? SalaryCurrency { get; set; }
        
        public string? Location { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public bool IsRemote { get; set; }
        
        public DateTime? ApplicationDeadline { get; set; }
        public DateTime PostedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? VacancyCount { get; set; }
        
        public List<JobSkillDto> Skills { get; set; } = new();
    }

    public class JobSkillDto
    {
        public Guid SkillId { get; set; }
        public string SkillName { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public int ImportanceWeight { get; set; }
        public string? MinimumProficiencyLevel { get; set; }
    }

    public class CreateJobRequest
    {
        [Required]
        public Guid JobCategoryId { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        public string? Responsibilities { get; set; }
        public string? Requirements { get; set; }
        
        [Required]
        public string EmploymentType { get; set; } = string.Empty; // Full-time, etc.
        
        public string? WorkMode { get; set; }
        public string? ExperienceLevel { get; set; }
        
        [Range(0, 50)]
        public int? MinimumExperienceYears { get; set; }
        
        [Range(0, 50)]
        public int? MaximumExperienceYears { get; set; }
        
        [Range(0, 10000000)]
        public decimal? MinimumSalary { get; set; }
        
        [Range(0, 10000000)]
        public decimal? MaximumSalary { get; set; }
        
        [MaxLength(10)]
        public string? SalaryCurrency { get; set; }
        
        public string? Location { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public bool IsRemote { get; set; }
        
        public DateTime? ApplicationDeadline { get; set; }
        public int? VacancyCount { get; set; }
        
        public List<CreateJobSkillRequest> Skills { get; set; } = new();
    }

    public class CreateJobSkillRequest
    {
        [Required]
        public Guid SkillId { get; set; }
        public bool IsRequired { get; set; } = false;
        public int ImportanceWeight { get; set; } = 1;
        public string? MinimumProficiencyLevel { get; set; }
    }

    public class UpdateJobRequest : CreateJobRequest
    {
        public string Status { get; set; } = "Active";
    }
}
