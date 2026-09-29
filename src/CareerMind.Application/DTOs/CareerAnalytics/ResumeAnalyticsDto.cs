using System.Collections.Generic;

namespace CareerMind.Application.DTOs.CareerAnalytics
{
    public class ResumeAnalyticsDto
    {
        public bool HasResume { get; set; }
        public decimal LatestATSScore { get; set; }
        public decimal? PreviousATSScore { get; set; }
        public decimal? ATSImprovement { get; set; }
        
        public decimal StructureScore { get; set; }
        public decimal SkillsScore { get; set; }
        public decimal KeywordsScore { get; set; }
        public decimal ExperienceScore { get; set; }
        public decimal EducationScore { get; set; }
        public decimal CompletenessScore { get; set; }
        
        public List<string> TopImprovementAreas { get; set; } = new List<string>();
    }
}
