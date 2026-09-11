using System;

namespace CareerMind.Application.DTOs.CareerGap
{
    public class SkillRoadmapItemDto
    {
        public int StepNumber { get; set; }
        public Guid SkillId { get; set; }
        public string SkillName { get; set; } = string.Empty;
        public string CurrentLevel { get; set; } = string.Empty;
        public string TargetLevel { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
