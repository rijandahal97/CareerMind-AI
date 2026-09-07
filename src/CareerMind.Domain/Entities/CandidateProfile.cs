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
        public string? ResumeUrl { get; set; }
        public ICollection<CandidateSkill> CandidateSkills { get; set; } = new List<CandidateSkill>();
        public ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
    }
}
