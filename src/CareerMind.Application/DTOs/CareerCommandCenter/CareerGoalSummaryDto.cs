using System.Collections.Generic;
using CareerMind.Application.DTOs.Job;

namespace CareerMind.Application.DTOs.CareerCommandCenter
{
    public class CareerGoalSummaryDto
    {
        public string CurrentRole { get; set; } = string.Empty;
        public string TargetRole { get; set; } = string.Empty;
        public string CareerCategory { get; set; } = string.Empty;
        public decimal ReadinessScore { get; set; }
        public decimal CompatibilityScore { get; set; }
        public string TransitionDifficulty { get; set; } = "MEDIUM";
        public List<string> TopSkills { get; set; } = new List<string>();
        public List<string> MissingSkills { get; set; } = new List<string>();
        public List<JobMatchResultDto> RecommendedTargetJobs { get; set; } = new List<JobMatchResultDto>();
    }
}
