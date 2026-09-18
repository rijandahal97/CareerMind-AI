using System;

namespace CareerMind.Application.DTOs.ApplicationOptimization
{
    public class ApplicationReadinessDto
    {
        public Guid JobId { get; set; }
        public double Score { get; set; }
        public string ReadinessLevel { get; set; } = string.Empty;
        
        public double ProfileAlignmentScore { get; set; }
        public double SkillAlignmentScore { get; set; }
        public double ExperienceAlignmentScore { get; set; }
        public double EducationAlignmentScore { get; set; }
        public double ResumeAlignmentScore { get; set; }
        public double InterviewReadinessScore { get; set; }

        public string? MissingDataBlockers { get; set; }
    }
}
