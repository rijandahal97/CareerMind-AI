using System.Collections.Generic;
using CareerMind.Domain.Enums;

namespace CareerMind.Domain.Entities
{
    public class Skill : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public SkillType SkillType { get; set; } = SkillType.Technical;
        public bool IsActive { get; set; } = true;

        public ICollection<CandidateSkill> CandidateSkills { get; set; } = new List<CandidateSkill>();
        public ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();
    }
}
