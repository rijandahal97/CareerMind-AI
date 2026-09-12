using System.Collections.Generic;

namespace CareerMind.Application.DTOs.CareerPath
{
    public class CareerRecommendationDto
    {
        public string CareerRole { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal CompatibilityScore { get; set; }
        public decimal ReadinessScore { get; set; }
        public string CareerLevel { get; set; } = string.Empty;
        public string TransitionDifficulty { get; set; } = string.Empty; // "LOW", "MEDIUM", "HIGH"
        public decimal SkillCoveragePercentage { get; set; }

        public List<string> Strengths { get; set; } = new();
        public List<CareerSkillRequirementDto> RequiredSkillsAnalysis { get; set; } = new();
        
        public string RecommendedNextAction { get; set; } = string.Empty;
        public List<string> Explanation { get; set; } = new();
    }
}
