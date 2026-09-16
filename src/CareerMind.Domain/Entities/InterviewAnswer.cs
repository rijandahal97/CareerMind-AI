using System;

namespace CareerMind.Domain.Entities
{
    public class InterviewAnswer : BaseEntity
    {
        public Guid InterviewQuestionId { get; set; }
        public InterviewQuestion InterviewQuestion { get; set; } = null!;

        public string AnswerText { get; set; } = string.Empty;

        public double? Score { get; set; }
        public double? RelevanceScore { get; set; }
        public double? TechnicalAccuracyScore { get; set; }
        public double? CommunicationScore { get; set; }
        public double? CompletenessScore { get; set; }
        public double? RoleAlignmentScore { get; set; }

        public string? Feedback { get; set; }
        public string? Strengths { get; set; }
        public string? Weaknesses { get; set; }
        public string? ImprovementSuggestion { get; set; }

        public DateTime? AnsweredAt { get; set; }
    }
}
