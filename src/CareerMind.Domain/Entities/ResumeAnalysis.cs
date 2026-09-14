using System;
using System.Collections.Generic;

namespace CareerMind.Domain.Entities
{
    public class ResumeAnalysis : BaseEntity
    {
        public Guid ResumeId { get; set; }
        public Resume Resume { get; set; } = null!;

        public Guid? TargetJobId { get; set; }
        // public Job TargetJob { get; set; } // optional navigation if needed

        // Scores (0-100)
        public int ATSScore { get; set; }
        public int StructureScore { get; set; }
        public int SkillsScore { get; set; }
        public int KeywordScore { get; set; }
        public int ExperienceScore { get; set; }
        public int EducationScore { get; set; }
        public int CompletenessScore { get; set; }

        // Detailed data
        public string? SkillsFound { get; set; }
        public string? MissingSkills { get; set; }
        public string? MissingKeywords { get; set; }
        public string? Strengths { get; set; }
        public string? Suggestions { get; set; }

        public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    }
}
