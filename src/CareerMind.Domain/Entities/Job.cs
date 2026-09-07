using System;
using System.Collections.Generic;

namespace CareerMind.Domain.Entities
{
    public class Job : BaseEntity
    {
        public Guid CompanyId { get; set; }
        public Company Company { get; set; } = null!;
        
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? EmploymentType { get; set; }
        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();
        public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
    }
}
