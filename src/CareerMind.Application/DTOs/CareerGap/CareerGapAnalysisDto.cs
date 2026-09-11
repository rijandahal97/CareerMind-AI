using System;
using System.Collections.Generic;

namespace CareerMind.Application.DTOs.CareerGap
{
    public class CareerGapAnalysisDto
    {
        public Guid JobId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;

        public decimal CurrentReadinessScore { get; set; }

        public int TotalRequiredSkills { get; set; }
        public int MatchedSkillsCount { get; set; }
        public int DevelopingSkillsCount { get; set; }
        public int MissingSkillsCount { get; set; }

        public List<string> StrongSkills { get; set; } = new();
        public List<string> DevelopingSkills { get; set; } = new();
        public List<string> MissingSkills { get; set; } = new();

        public List<SkillGapDetailDto> HighPriorityGaps { get; set; } = new();
        public List<SkillGapDetailDto> MediumPriorityGaps { get; set; } = new();
        public List<SkillGapDetailDto> LowPriorityGaps { get; set; } = new();

        public List<string> CareerInsights { get; set; } = new();

        public List<SkillRoadmapItemDto> Roadmap { get; set; } = new();
    }
}
