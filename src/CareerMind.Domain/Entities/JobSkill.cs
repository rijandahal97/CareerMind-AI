using System;

namespace CareerMind.Domain.Entities
{
    public class JobSkill
    {
        public Guid JobId { get; set; }
        public Job Job { get; set; } = null!;
        
        public Guid SkillId { get; set; }
        public Skill Skill { get; set; } = null!;
        
        public bool IsRequired { get; set; } = false;
        public int ImportanceWeight { get; set; } = 1; // 1 to 5 (least to most important)
        public string? MinimumProficiencyLevel { get; set; } // Beginner, Intermediate, Expert
    }
}
