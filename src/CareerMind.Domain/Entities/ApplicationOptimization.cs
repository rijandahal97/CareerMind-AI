using System;
using System.Collections.Generic;
using CareerMind.Domain.Enums;

namespace CareerMind.Domain.Entities
{
    public class ApplicationOptimization : BaseEntity
    {
        public Guid CandidateProfileId { get; set; }
        public CandidateProfile CandidateProfile { get; set; } = null!;

        public Guid JobId { get; set; }
        public Job Job { get; set; } = null!;

        public Guid? ResumeId { get; set; }
        public Resume? Resume { get; set; }

        public double? ApplicationReadinessScore { get; set; }
        public double? ProfileAlignmentScore { get; set; }
        public double? ResumeAlignmentScore { get; set; }
        public double? SkillAlignmentScore { get; set; }
        public double? ExperienceAlignmentScore { get; set; }
        public double? EducationAlignmentScore { get; set; }
        public double? InterviewReadinessScore { get; set; }

        public ApplicationStatus ApplicationStatus { get; set; } = ApplicationStatus.Draft;
        public bool IsRecommendedToApply { get; set; }
        public int OptimizationVersion { get; set; } = 1;

        public string? OptimizedHeadline { get; set; }
        public string? OptimizedSummary { get; set; }
        public string? CoverLetter { get; set; }

        public ICollection<ApplicationOptimizationSuggestion> Suggestions { get; set; } = new List<ApplicationOptimizationSuggestion>();
    }
}
