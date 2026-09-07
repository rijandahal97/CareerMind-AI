using System;

namespace CareerMind.Domain.Entities
{
    public class CandidateSkill
    {
        public Guid CandidateProfileId { get; set; }
        public CandidateProfile CandidateProfile { get; set; } = null!;
        
        public Guid SkillId { get; set; }
        public Skill Skill { get; set; } = null!;
        
        public int? YearsOfExperience { get; set; }
    }
}
