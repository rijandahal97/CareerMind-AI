using System;

namespace CareerMind.Application.DTOs.CareerCommandCenter
{
    public class ResumeHealthSummaryDto
    {
        public Guid? PrimaryResumeId { get; set; }
        public string? FileName { get; set; }
        public decimal OverallScore { get; set; }
        public decimal StructureScore { get; set; }
        public decimal SkillsScore { get; set; }
        public decimal KeywordsScore { get; set; }
        public decimal ExperienceScore { get; set; }
        public decimal EducationScore { get; set; }
        public decimal CompletenessScore { get; set; }
        public bool HasResume { get; set; }
        public DateTime? LastAnalyzedAt { get; set; }
    }
}
