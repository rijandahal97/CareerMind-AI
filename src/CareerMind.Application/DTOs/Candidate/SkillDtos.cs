using System;
using CareerMind.Domain.Enums;

namespace CareerMind.Application.DTOs.Candidate
{
    public class SkillDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public SkillType SkillType { get; set; }
    }

    public class CandidateSkillDto
    {
        public Guid Id { get; set; }
        public Guid SkillId { get; set; }
        public string SkillName { get; set; } = string.Empty;
        public ProficiencyLevel ProficiencyLevel { get; set; }
        public int? YearsOfExperience { get; set; }
        public int? LastUsedYear { get; set; }
        public bool IsPrimary { get; set; }
    }

    public class AddCandidateSkillDto
    {
        public Guid SkillId { get; set; }
        public ProficiencyLevel ProficiencyLevel { get; set; }
        public int? YearsOfExperience { get; set; }
        public int? LastUsedYear { get; set; }
        public bool IsPrimary { get; set; }
    }

    public class UpdateCandidateSkillDto
    {
        public ProficiencyLevel ProficiencyLevel { get; set; }
        public int? YearsOfExperience { get; set; }
        public int? LastUsedYear { get; set; }
        public bool IsPrimary { get; set; }
    }
}
