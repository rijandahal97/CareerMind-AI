using System;
using System.Collections.Generic;

namespace CareerMind.Domain.Entities
{
    public class Job : BaseEntity
    {
        public Guid EmployerProfileId { get; set; }
        public EmployerProfile EmployerProfile { get; set; } = null!;
        
        public Guid JobCategoryId { get; set; }
        public JobCategory JobCategory { get; set; } = null!;
        
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Responsibilities { get; set; }
        public string? Requirements { get; set; }
        
        public string? EmploymentType { get; set; } // Full-time, Part-time, Contract, Internship
        public string? WorkMode { get; set; } // On-site, Hybrid, Remote
        public string? ExperienceLevel { get; set; } // Entry, Mid, Senior, Executive
        
        public int? MinimumExperienceYears { get; set; }
        public int? MaximumExperienceYears { get; set; }
        
        public decimal? MinimumSalary { get; set; }
        public decimal? MaximumSalary { get; set; }
        public string? SalaryCurrency { get; set; } // USD, EUR, etc.
        
        public string? Location { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public bool IsRemote { get; set; }
        
        public DateTime? ApplicationDeadline { get; set; }
        public DateTime PostedAt { get; set; } = DateTime.UtcNow;
        
        public string Status { get; set; } = "Active"; // Active, Closed, Draft
        public int? VacancyCount { get; set; }

        public ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();
        public ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
        public ICollection<SavedJob> SavedJobs { get; set; } = new List<SavedJob>();
    }
}
