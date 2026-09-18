using System;
using System.Collections.Generic;
using CareerMind.Domain.Enums;

namespace CareerMind.Application.DTOs.ApplicationOptimization
{
    public class ApplicationOptimizationDto
    {
        public Guid Id { get; set; }
        public Guid CandidateProfileId { get; set; }
        public Guid JobId { get; set; }
        public Guid? ResumeId { get; set; }
        
        public double? ApplicationReadinessScore { get; set; }
        public double? ProfileAlignmentScore { get; set; }
        public double? ResumeAlignmentScore { get; set; }
        public double? SkillAlignmentScore { get; set; }
        public double? ExperienceAlignmentScore { get; set; }
        public double? EducationAlignmentScore { get; set; }
        public double? InterviewReadinessScore { get; set; }

        public ApplicationStatus ApplicationStatus { get; set; }
        public bool IsRecommendedToApply { get; set; }
        public int OptimizationVersion { get; set; }

        public string? OptimizedHeadline { get; set; }
        public string? OptimizedSummary { get; set; }
        public string? CoverLetter { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<ApplicationOptimizationSuggestionDto> Suggestions { get; set; } = new();
    }
}
