using System;

namespace CareerMind.Application.DTOs.CareerCommandCenter
{
    public class CareerActionDto
    {
        public string Id { get; set; } = string.Empty;
        public string ActionKey { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "General"; // Skill Development, Resume, Portfolio, Experience, Job Search, Interview Preparation, Profile, Career Planning
        public string Priority { get; set; } = "Medium"; // Critical, High, Medium, Low
        public int TargetDays { get; set; } // 30, 60, 90
        public string EstimatedEffort { get; set; } = "1 week";
        public string EstimatedImpact { get; set; } = "High";
        public string? RelatedSkill { get; set; }
        public string? RelatedModule { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
