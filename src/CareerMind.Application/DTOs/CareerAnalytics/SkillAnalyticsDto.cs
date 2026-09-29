using System.Collections.Generic;

namespace CareerMind.Application.DTOs.CareerAnalytics
{
    public class SkillAnalyticsDto
    {
        public int TotalSkills { get; set; }
        public int StrongSkills { get; set; }
        public int DevelopingSkills { get; set; }
        public int MissingTargetSkills { get; set; }
        public decimal SkillDevelopmentProgress { get; set; }
        public Dictionary<string, int> SkillsDistribution { get; set; } = new Dictionary<string, int>();
    }
}
