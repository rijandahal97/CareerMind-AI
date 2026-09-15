using System.Collections.Generic;

namespace CareerMind.Application.DTOs.CareerCommandCenter
{
    public class CareerReadinessDto
    {
        public decimal OverallScore { get; set; }
        public decimal ProfileScore { get; set; }
        public decimal SkillScore { get; set; }
        public decimal CareerPathScore { get; set; }
        public decimal JobMatchScore { get; set; }
        public decimal ResumeScore { get; set; }
        public string ReadinessLevel { get; set; } = "Developing"; // Beginner, Developing, Job Ready, Highly Competitive
        public List<string> ScoreBreakdownExplanations { get; set; } = new List<string>();
    }
}
