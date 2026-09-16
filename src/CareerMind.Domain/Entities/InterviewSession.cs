using System;
using System.Collections.Generic;
using CareerMind.Domain.Enums;

namespace CareerMind.Domain.Entities
{
    public class InterviewSession : BaseEntity
    {
        public Guid CandidateProfileId { get; set; }
        public CandidateProfile CandidateProfile { get; set; } = null!;

        public Guid? JobId { get; set; }
        public Job? TargetJob { get; set; }

        public string SessionTitle { get; set; } = string.Empty;
        public InterviewType InterviewType { get; set; } = InterviewType.Mixed;
        public InterviewDifficulty Difficulty { get; set; } = InterviewDifficulty.Intermediate;

        public double? OverallReadinessScore { get; set; }
        public double? TechnicalScore { get; set; }
        public double? CommunicationScore { get; set; }
        public double? RoleAlignmentScore { get; set; }
        public double? CompletenessScore { get; set; }

        public string? Strengths { get; set; }
        public string? Weaknesses { get; set; }
        public string? Recommendations { get; set; }

        public InterviewStatus Status { get; set; } = InterviewStatus.NotStarted;

        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public ICollection<InterviewQuestion> Questions { get; set; } = new List<InterviewQuestion>();
    }
}
