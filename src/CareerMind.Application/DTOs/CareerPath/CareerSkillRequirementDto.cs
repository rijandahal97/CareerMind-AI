using System.Collections.Generic;

namespace CareerMind.Application.DTOs.CareerPath
{
    public class CareerSkillRequirementDto
    {
        public string SkillName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // "STRONG", "DEVELOPING", "MISSING"
        public string Priority { get; set; } = string.Empty; // "HIGH", "MEDIUM", "LOW"
    }
}
