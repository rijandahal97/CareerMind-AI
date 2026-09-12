using System.Collections.Generic;

namespace CareerMind.Application.DTOs.CareerPath
{
    public class CareerPathStageDto
    {
        public string Role { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public decimal ReadinessPercentage { get; set; }
        public bool IsCurrentRole { get; set; }
        
        public List<string> RequiredSkills { get; set; } = new();
        public List<string> CandidateSkills { get; set; } = new();
        public List<string> MissingSkills { get; set; } = new();
        public List<string> KeySkillsToImprove { get; set; } = new();
        
        public string SuggestedNextStep { get; set; } = string.Empty;
    }
}
