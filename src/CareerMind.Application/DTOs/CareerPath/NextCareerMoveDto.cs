using System.Collections.Generic;

namespace CareerMind.Application.DTOs.CareerPath
{
    public class NextCareerMoveDto
    {
        public string TargetRole { get; set; } = string.Empty;
        public decimal ReadinessPercentage { get; set; }
        
        public List<string> WhyReasons { get; set; } = new();
        public List<CareerSkillRequirementDto> FocusNext { get; set; } = new();
        
        public string Action { get; set; } = string.Empty;
    }
}
